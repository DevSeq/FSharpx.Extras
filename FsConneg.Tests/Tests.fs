﻿module Tests

open Xunit
open FsConneg

[<Fact>]
let ``Parse AcceptLanguage``() =
    let acceptLang = "en-US; q=0.6, de-de;q=0.4,de;q=0.2,en-ca,en;q=0.8, pepe;q=0"
    match parseAccept acceptLang with
    | ["en-ca"; "en"; "en-us"; "de-de"; "de"] -> ()
    | x -> failwithf "wrong parsing: %A" x

[<Fact>]
let ``Parse Accept``() =
    let accept = "text/html; q=0.8; level=2, text/html; q=0.2; level=1"
    match parseAccept accept with
    | ["text/html;level=2"; "text/html;level=1"] -> ()
    | x -> failwithf "wrong parsing: %A" x

[<Fact>]
let ``Parse Accept with implicit precedence``() =
    // http://www.w3.org/Protocols/rfc2616/rfc2616-sec14.html#sec14.1
    let accept = "text/*, text/html, text/html;level=1, */*"
    match parseAccept accept with
    | ["text/html;level=1"; "text/html"; "text/*"; "*/*"] -> ()
    | x -> failwithf "wrong parsing: %A" x

[<Fact>]
let ``Best content type``() =
    let accept = "text/html; q=0.8; level=2, text/html; q=0.2; level=1, text/plain, image/jpeg"
    match bestMediaType "text" accept with
    | None -> failwith "no suitable content type found"
    | Some contentType -> Assert.Equal("text/plain", contentType)

[<Fact>]
let ``Best content type, none found``() =
    let accept = "text/html; q=0.8; level=2, text/html; q=0.2; level=1, text/plain, image/jpeg"
    Assert.Equal(None, bestMediaType "application" accept)

[<Fact>]
let ``Content type matching``() =
    let accept = "text/html; q=0.8; level=2, text/html; q=0.2; level=1, image/png, image/jpeg;q=0.8"
    let images = parseAccept accept |> List.filter (fun t -> t.StartsWith "image/")
    match images with
    | "image/jpeg"::_ -> failwith "should have been image/png"
    | "image/png"::_ -> ()
    | x -> failwithf "can't handle any of these content types: %A, failing with 406" x
