// ----------------------------------------------------------------------------
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.
// ----------------------------------------------------------------------------
namespace Test.Yaaf.MessageArchiveManager

open NUnit.Framework
open Yaaf.Helper
open Yaaf.TestHelper

open Yaaf.Xmpp
open Swensen.Unquote
open Yaaf.Xmpp.MessageArchiving

[<AbstractClass>]
type PreferenceStoreTests() =
    inherit MyTestClass()

    let defUser = (JabberId.Parse "user@nunit.org")
    abstract CreatePreferenceStore: JabberId -> IUserPreferenceStore

    override x.Setup () = 
        base.Setup()

    override x.TearDown() = 
        base.TearDown()
        
        
    [<Test>]
    member x.``Check that storing and deleting default preferences works``() =
        let prefStore = x.CreatePreferenceStore defUser
        test <@ prefStore.GetUserPreferences () |> waitTask = None @>

        prefStore.SetDefaultOtrSaveMode (Some { Expire = Some 123L; OtrMode = Some OtrMode.Concede; SaveMode = Some SaveMode.Body }) |> Task.ofPlainTask |> waitTask
        let results = prefStore.GetUserPreferences () |> waitTask
        test <@ results.IsSome @>
        let saveMode = results.Value.DefaultOtrSaveMode
        test <@ saveMode.Expire = Some 123L @>
        test <@ saveMode.OtrMode = Some OtrMode.Concede @>
        test <@ saveMode.SaveMode = Some SaveMode.Body @>
        prefStore.SetDefaultOtrSaveMode None |> Task.ofPlainTask |> waitTask

        let results = prefStore.GetUserPreferences () |> waitTask
        test <@ results.IsNone @>