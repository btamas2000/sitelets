// $begin{copyright}
//
// This file is part of WebSharper
//
// Copyright (c) 2008-2018 IntelliFactory
//
// Licensed under the Apache License, Version 2.0 (the "License"); you
// may not use this file except in compliance with the License.  You may
// obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or
// implied.  See the License for the specific language governing
// permissions and limitations under the License.
//
// $end{copyright}

namespace Sitelets

/// Defines HTTP-related functionality.
module HttpHelpers =
    open System
    open System.Collections.Generic
    open System.Collections.Specialized
    open System.IO
    open System.Threading.Tasks
    open Microsoft.AspNetCore.Http

    type SV = Microsoft.Extensions.Primitives.StringValues
    
    let CollectionToMap<'T when 'T :> IEnumerable<KeyValuePair<string, SV>>> (c: 'T) : Map<string, string> =
        c
        |> Seq.map (fun i ->
            i.Key, i.Value.ToString()
        )
        |> Map.ofSeq
    
    let CollectionFromMap<'T when 'T :> IEnumerable<KeyValuePair<string, SV>>> (m: Map<string, string>) : IEnumerable<KeyValuePair<string, SV>> =
        m
        |> Seq.map (fun (KeyValue(k,v)) ->
            KeyValuePair (k, Microsoft.Extensions.Primitives.StringValues v)
            )

    let ToQuery (req: HttpRequest) : Map<string, string> =
        let returnMap : Map<string, string> = Map.empty
        for KeyValue(k, v) in req.Query do
            returnMap.Add (k, v.ToString()) |> ignore
        returnMap

    let ToForm (req: HttpRequest) : Map<string, string> =
        if req.HasFormContentType then
            let returnMap : Map<string, string> = Map.empty
            for KeyValue(k, v) in req.Query do
                returnMap.Add (k, v.ToString()) |> ignore
            returnMap
        else
            Map.empty

    let SetUri (r: HttpRequest) =
        System.UriBuilder(
            r.Scheme,
            r.Host.Host,
            r.Host.Port.GetValueOrDefault(-1),
            r.Path.ToString(),
            r.QueryString.ToString()
        ).Uri