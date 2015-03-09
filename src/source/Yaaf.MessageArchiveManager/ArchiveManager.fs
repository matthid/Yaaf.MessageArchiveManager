// ----------------------------------------------------------------------------
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.
// ----------------------------------------------------------------------------
namespace Yaaf.Xmpp.MessageArchiveManager

open System
open Yaaf.FSharp.Control
open Yaaf.Helper
open Yaaf.Xmpp.MessageArchiving

type JabberId = Yaaf.Xmpp.JabberId

type MessageCollectionHeader = Yaaf.Xmpp.MessageArchiving.ChatCollectionId

type MessageCollection = Yaaf.Xmpp.MessageArchiving.ChatCollection

/// Simple Query syntax for messages within the Archive
type Query = 
    | And of Query * Query
    | Or of Query * Query
    | Not of Query
    | OnDate of DateTime
    | Between of DateTime * TimeSpan
    | ChangedSince of DateTime
    | With of JabberId
    | Contains of string

type IWriteOnlyUserArchivingStore =
    abstract StoreCollection : Yaaf.Xmpp.MessageArchiving.ChatCollection -> System.Threading.Tasks.Task

type IWriteOnlyMessageArchivingStore = 
    abstract GetArchiveStore : JabberId -> IWriteOnlyUserArchivingStore System.Threading.Tasks.Task 

/// Interface to manage your Chat-Archive.
/// This interface should allow anybody to implement http://xmpp.org/extensions/xep-0136.html or http://xmpp.org/extensions/xep-0313.html on top
/// It is also possible to use another server with http://xmpp.org/extensions/xep-0136.html as backend!
/// But also multiple frontends are possible (Webserver for example).
type IExtendedUserArchivingStore = 
    // The IUserArchivingStore interface is actually everything needed to implement xep-0136
    // however we add some more methods below to be able to better manage the archive (for example via scripting or other frontends)
    inherit Yaaf.Xmpp.MessageArchiving.IUserArchivingStore
    
    /// Queries the message archive for all messages that match the given query (use None for all).
    abstract QueryMessages : Query option -> AsyncSeq<MessageCollection>
    
    /// Deletes the MessageCollections within the specified range (or only one if the secound parameter is None)
    /// Deletes ALL MessageCollections when both parameter are None.
    abstract RemoveCollections : Query option -> Async<unit>
    
type CombiningMessageArchivingStore (writeOnly:IWriteOnlyMessageArchivingStore list, defStore : Yaaf.Xmpp.MessageArchiving.IMessageArchivingStore) = 
    interface System.IDisposable with
        member x.Dispose() =
            //for (imap:ImapConnection) in userColStore.Values do
            for w in writeOnly |> Seq.cast<obj> |> Seq.append [defStore] do
              try
                match w with
                | :? IDisposable as d -> d.Dispose()
                | _ -> ()
              with exn ->
                  System.Console.Error.WriteLine(sprintf "Error on dispose: %O" exn)
                  //|> Option.map raise |> ignore

    interface IMessageArchivingStore with
        member x.GetPreferenceStore (jid:JabberId) = defStore.GetPreferenceStore jid

        member x.GetArchiveStore (jid:JabberId) =
            async {
                let! writeStores =
                  Async.Parallel (writeOnly |> Seq.map (fun w -> w.GetArchiveStore jid |> Task.await))
                
                let! defStore = defStore.GetArchiveStore jid |> Task.await
                return 
                    { new IUserArchivingStore with
                        member x.StoreCollection col =
                            for store in writeStores do
                              store.StoreCollection col |> ignore
                            defStore.StoreCollection col
                        member x.FilterMessages filter = defStore.FilterMessages filter
                        member x.RetrieveCollection id = defStore.RetrieveCollection id
                        member x.RemoveCollection id = defStore.RemoveCollection id
                        member x.GetChangesSince date = defStore.GetChangesSince date
                    }
            }
            |> Async.StartAsTaskImmediate

module ArchivingStore =
  open Yaaf.Xmpp.MessageArchiving

  let WriteOnlyUserStore (store:IUserArchivingStore) =
    { new IWriteOnlyUserArchivingStore with 
        member x.StoreCollection col = store.StoreCollection col }
  let WriteOnly (store:IMessageArchivingStore) = 
    { new IWriteOnlyMessageArchivingStore with
        member x.GetArchiveStore jid = 
          async { 
            let! store = store.GetArchiveStore jid |> Task.await
            return WriteOnlyUserStore store } |> Async.StartAsTaskImmediate }

  let Combine (writeOnly:IWriteOnlyMessageArchivingStore list, normal:IMessageArchivingStore) =
    new CombiningMessageArchivingStore(writeOnly, normal) :> IMessageArchivingStore