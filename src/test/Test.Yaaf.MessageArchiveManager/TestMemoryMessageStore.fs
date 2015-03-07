// ----------------------------------------------------------------------------
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.
// ----------------------------------------------------------------------------
namespace Test.Yaaf.Xmpp.IM

open FsUnit
open NUnit.Framework
open Test.Yaaf.Xmpp
open Yaaf.Xmpp
open Yaaf.Xmpp.IM
open Yaaf.Helper
open Yaaf.TestHelper
open Yaaf.Xmpp.MessageArchiving
open Yaaf.Xmpp.IM.Server
open Swensen.Unquote

/// This base class provides some basic unit tests which should work on all instances of MessageArchivingStore
/// So everyone can just inherit from this class and use those tests for their own implementation
[<TestFixture>]
type TestMemoryMessageStore () = 
    inherit MessageArchivingStoreTest()
    
    override x.CreateArchivingStore (jid:JabberId) = MemoryUserArchivingStore(jid.BareJid) :> IUserArchivingStore
