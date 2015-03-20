(*

//------------------------------------------
// Step 0. Get the package bootstrap

open System
open System.IO

Environment.CurrentDirectory <- __SOURCE_DIRECTORY__

if not (File.Exists "paket.exe") then
    let url = "https://github.com/fsprojects/Paket/releases/download/0.27.2/paket.exe"
    use wc = new Net.WebClient()
    let tmp = Path.GetTempFileName()
    wc.DownloadFile(url, tmp)
    File.Move(tmp,Path.GetFileName url);;

// Step 1. Resolve and install the packages 

#r "paket.exe"

Paket.Dependencies.Install """
    source https://nuget.org/api/v2
    nuget Suave
    nuget FSharp.Data 
    nuget FSharp.Charting
""";;

*)

printfn "starting..."
#r "packages/Suave/lib/net40/Suave.dll"
#r "packages/FSharp.Data/lib/net40/FSharp.Data.dll"
//#r "packages/FSharp.Charting/lib/net40/FSharp.Charting.dll"

//let ctxt = FSharp.Data.WorldBankData.GetDataContext()

//let data = ctxt.Countries.Algeria.Indicators.``GDP (current US$)``

open Suave                 // always open suave
open Suave.Http.Successful // for OK-result
open Suave.Web             // for config
open Suave.Types             

let config = 
    { defaultConfig with 
        logger = Logging.Loggers.saneDefaultsFor Logging.LogLevel.Verbose
        bindings=[ HttpBinding.mk' HTTP  "0.0.0.0" 5000 ] }

//let startWebServer c rsp = async { do webServer c rsp } |> Async.Start



let angularHeader = """<head>
<link rel="stylesheet" href="http://maxcdn.bootstrapcdn.com/bootstrap/3.2.0/css/bootstrap.min.css">
<script src="http://ajax.googleapis.com/ajax/libs/angularjs/1.2.26/angular.min.js"></script>
</head>"""

let fancyText = 
    [ yield """<html>"""
      yield angularHeader
      yield """ <body>"""
      yield """  <table class="table table-striped">"""
      yield """   <thead><tr><th>Category</th><th>Count</th></tr></thead>"""
      yield """   <tbody>"""
      for (category,count) in [ "abc", 100 ] do
         yield sprintf "<tr><td>%s</td><td>%d</td></tr>" category count 
      yield """   </tbody>"""
      yield """  </table>"""
      yield """ </body>""" 
      yield """</html>""" ]
    |> String.concat "\n"

printfn "starting server..."
eprintfn "starting server (err)..."

startWebServer config (OK fancyText)
printfn "exiting server..."
eprintfn "exiting server (err)..."


