module UnitTests

open NUnit.Framework
open FsUnit
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Mvc.Testing
open Sitelets
open TestSitelets

[<SetUp>]
let Setup () =
    ()

type TestEndPoint =
    | Ep1
    | Ep2

type TestEndPoint2 =
    | [<EndPoint "/mappedendpoint1">] Ep1
    | [<EndPoint "/mappedendpoint2">] Ep2

type TestInferEndpoint =
    | [<EndPoint "/infer1">] E1
    | [<EndPoint "/infer2">] E2
    | [<EndPoint "/infer3">] E3

module UnitTestHelpers =
    let helloWorldSitelet =
        Sitelet.Content "/test1" TestEndPoint.Ep1 (fun ctx -> box "Hello World")

    let helloWorldSitelet2 =
        Sitelet.Content "/test2" TestEndPoint.Ep2 (fun ctx -> box "Hello, World!")

    let sampleHttpRequest() =
        let ctx = DefaultHttpContext()
        let httpReq = ctx.Request
        httpReq.Scheme <- "http"
        httpReq.Method <- "GET"
        httpReq.Host <- HostString("localhost:5000")
        httpReq.Path <- PathString("/test1")
        httpReq

module TH = UnitTestHelpers

[<Test; Category("Hello World Tests")>]
let ``Hello World routing test`` () =
    let req = TH.sampleHttpRequest ()

    TH.helloWorldSitelet.Router.Route <| RoutedHttpRequest req
    |> should equal (Some TestEndPoint.Ep1)
    
    req.Path <- PathString("/test2")
    TH.helloWorldSitelet.Router.Route <| RoutedHttpRequest req
    |> should equal None

[<Test; Category("Hello World Tests")>]
let ``Hello World linking test`` () =
    let link = TH.helloWorldSitelet.Router.Link(TestEndPoint.Ep1)
    link |> should be (ofCase <@ Some @>)

    link.Value.ToString() |> should equal "/test1"

    let badlink = TH.helloWorldSitelet.Router.Link(TestEndPoint.Ep2)
    badlink |> should equal None

[<Test; Category("Sitelet Tests")>]
let ``Shifting test`` () =
    let shiftedSite = TH.helloWorldSitelet.Shift "shifted"
    let req = TH.sampleHttpRequest ()
    let link = shiftedSite.Router.Link(TestEndPoint.Ep1)
    link.Value.ToString() |> should equal "/shifted/test1"
    req.Path <- PathString("/shifted/test1")
    let routedReq = shiftedSite.Router.Route <| RoutedHttpRequest req
    routedReq |> should equal (Some TestEndPoint.Ep1)

    let furtherShiftedSite = shiftedSite.Shift "extrashift"
    let req = TH.sampleHttpRequest ()
    let link = furtherShiftedSite.Router.Link(TestEndPoint.Ep1)
    link.Value.ToString() |> should equal "/extrashift/shifted/test1"
    req.Path <- PathString("/extrashift/shifted/test1")
    let fortherRoutedReq = furtherShiftedSite.Router.Route <| RoutedHttpRequest req
    fortherRoutedReq |> should equal (Some TestEndPoint.Ep1)

[<Test; Category("Sitelet Tests")>]
let ``Infer test`` () =
    let inferSitelet =
        Sitelet.Infer (fun ctx -> function
            | E1 -> box "Infer endpoint 1"
            | E2 -> box "Infer endpoint 2"
            | E3 -> box "Infer endpoint 3"
        )
    let link1 = inferSitelet.Router.Link TestInferEndpoint.E1
    link1.Value.ToString() |> should equal "/infer1"
    let link2 = inferSitelet.Router.Link TestInferEndpoint.E2
    link2.Value.ToString() |> should equal "/infer2"
    let link3 = inferSitelet.Router.Link TestInferEndpoint.E3
    link3.Value.ToString() |> should equal "/infer3"

    let req = TH.sampleHttpRequest ()
    req.Path <- PathString "/infer1"
    inferSitelet.Router.Route <| RoutedHttpRequest req |> should equal (Some TestInferEndpoint.E1)
    req.Path <- PathString "/infer2"
    inferSitelet.Router.Route <| RoutedHttpRequest req |> should equal (Some TestInferEndpoint.E2)
    req.Path <- PathString "/infer3"
    inferSitelet.Router.Route <| RoutedHttpRequest req |> should equal (Some TestInferEndpoint.E3)

[<Test; Category("Sitelet Tests")>]
let ``Sum Test`` () =
    let summedSitelet =
        Sitelet.Sum [
            TH.helloWorldSitelet
            TH.helloWorldSitelet2
        ]
    let link1 = summedSitelet.Router.Link TestEndPoint.Ep1
    link1.Value.ToString() |> should equal "/test1"
    let link2 = summedSitelet.Router.Link TestEndPoint.Ep2
    link2.Value.ToString() |> should equal "/test2"
    let req = TH.sampleHttpRequest ()
    summedSitelet.Router.Route <| RoutedHttpRequest req
    |> should equal (Some TestEndPoint.Ep1)
    req.Path <- PathString "/test2"
    summedSitelet.Router.Route <| RoutedHttpRequest req
    |> should equal (Some TestEndPoint.Ep2)

    let shiftedHelloWorldSite =
        TH.helloWorldSitelet.Shift "shifted"
    let req2 = TH.sampleHttpRequest ()
    let shiftedSum = 
        Sitelet.Sum [
            shiftedHelloWorldSite
            TH.helloWorldSitelet
            TH.helloWorldSitelet2
        ]
    req.Path <- PathString "/shifted/test1"
    shiftedSum.Router.Route <| RoutedHttpRequest req
    |> should equal (Some TestEndPoint.Ep1)
    req.Path <- PathString "/test1"
    shiftedSum.Router.Route <| RoutedHttpRequest req
    |> should equal (Some TestEndPoint.Ep1)
    req.Path <- PathString "/test2"
    shiftedSum.Router.Route <| RoutedHttpRequest req
    |> should equal (Some TestEndPoint.Ep2)
    req.Path <- PathString "/shifted/test2"
    shiftedSum.Router.Route <| RoutedHttpRequest req
    |> should equal None

[<Test; Category("Sitelet Tests")>]
let ``EmbedInUnion Test`` () =
    let sitelet = Sitelet.EmbedInUnion <@ HasSubEndPoint.Sub1 @> subEPSite
    let link1 = sitelet.Router.Link (HasSubEndPoint.Sub1 <| SubEndPoint.Action1)
    link1 |> should be (ofCase <@ Some @>)
    link1.Value.ToString() |> should equal "/sub/act1"
    let link2 = sitelet.Router.Link (HasSubEndPoint.Sub1 <| SubEndPoint.Action2)
    link2 |> should be (ofCase <@ Some @>)
    link2.Value.ToString() |> should equal "/sub/act2"
    let link3 = sitelet.Router.Link (HasSubEndPoint.Sub1 <| SubEndPoint.Action3)
    link3 |> should be (ofCase <@ Some @>)
    link3.Value.ToString() |> should equal "/sub/act3"

    let req = TH.sampleHttpRequest ()
    req.Path <- PathString "/sub/act1"
    sitelet.Router.Route <| RoutedHttpRequest req
    |> should equal (Some <| HasSubEndPoint.Sub1 SubEndPoint.Action1)
    req.Path <- PathString "/sub/act2"
    sitelet.Router.Route <| RoutedHttpRequest req
    |> should equal (Some <| HasSubEndPoint.Sub1 SubEndPoint.Action2)
    req.Path <- PathString "/sub/act3"
    sitelet.Router.Route <| RoutedHttpRequest req
    |> should equal (Some <| HasSubEndPoint.Sub1 SubEndPoint.Action3)

[<Test; Category("Sitelet Tests")>]
let ``InferPartialInUnion Test`` () =
    let sitelet = Sitelet.InferPartialInUnion <@ HasSubEndPoint.Sub2 @> (fun ctx ep -> box "Yes")
    let link = sitelet.Router.Link (HasSubEndPoint.Sub2 <| SubEndPoint2.SampleEp)
    link |> should be (ofCase <@ Some @>)
    link.Value.ToString() |> should equal "/sub/sampleep"

    let req = TH.sampleHttpRequest ()
    req.Path <- PathString "/sub/sampleep"
    sitelet.Router.Route <| RoutedHttpRequest req
    |> should equal (Some <| HasSubEndPoint.Sub2 SubEndPoint2.SampleEp)
    req.Path <- PathString "/sub/act1"
    sitelet.Router.Route <| RoutedHttpRequest req
    |> should equal None

//[<Test>]
//let ``Map test`` () =
//    let sitelet = Sitelet.Map (fun _ -> TestEndPoint2.Ep1) (fun _ -> TestEndPoint.Ep1) TH.helloWorldSitelet
//    let link = sitelet.Router.Link TestEndPoint2.Ep1
//    link.Value.ToString() |> should equal "/test1"

//    let req = TH.sampleHttpRequest ()
//    req.Path <- PathString "/mapped"
//    sitelet.Router.Route 