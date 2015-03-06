// ----------------------------------------------------------------------------
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.
// ----------------------------------------------------------------------------

namespace Test.Yaaf.Xmpp.MessageArchiveManager
open System
open NUnit.Framework
open FsUnit

[<TestFixture>]
type Test() = 
    [<Test>]
    member this.TestCase  () =        
        true |> should be True
        "On" |> should equal "On"
        ()

