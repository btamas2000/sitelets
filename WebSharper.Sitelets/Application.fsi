// $begin{copyright}
//
// This file is part of WebSharper
//
// Copyright (c) 2008-2016 IntelliFactory
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

module SPA =
    type EndPoint

open System
open System.Runtime.CompilerServices
open System.Threading.Tasks
open Microsoft.AspNetCore.Http

[<Class>]
type Application =
    /// Create a multi-page application.
    static member BaseMultiPage : (HttpContext -> 'EndPoint -> obj) -> Sitelet<'EndPoint>

    /// Create a single-page HTML application.
    static member BaseSinglePage : (HttpContext -> obj) -> Sitelet<SPA.EndPoint>

    /// Create a single-page application that returns text.
    // static member Text : (Context<SPA.EndPoint> -> string) -> Sitelet<SPA.EndPoint>

    /// Create a multi-page application.
    static member MultiPage : Func<HttpContext, 'EndPoint, obj> -> Sitelet<'EndPoint>

    /// Create a single-page HTML application.
    static member SinglePage : Func<HttpContext, obj> -> Sitelet<SPA.EndPoint>

    /// Create a single-page application that returns text.
    static member Text : Func<HttpContext, string> -> Sitelet<SPA.EndPoint>