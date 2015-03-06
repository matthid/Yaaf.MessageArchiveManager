// ----------------------------------------------------------------------------
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.
// ----------------------------------------------------------------------------
namespace Yaaf.Xmpp.MessageArchiveManager

open System
open Yaaf.FSharp.Control

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
