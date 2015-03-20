#if BOOTSTRAP
System.Environment.CurrentDirectory <- __SOURCE_DIRECTORY__
if not (System.IO.File.Exists "paket.exe") then let url = "https://github.com/fsprojects/Paket/releases/download/0.27.2/paket.exe" in use wc = new System.IO.Net.WebClient() in let tmp = System.IO.Path.GetTempFileName() in wc.DownloadFile(url, tmp); System.IO.File.Move(tmp,System.IO.Path.GetFileName url);;
#r "paket.exe"
Paket.Dependencies.Install (System.IO.File.ReadAllText "paket.dependencies")
#endif

//---------------------------------------------------------------------

#I "packages/Suave/lib/net40"
#r "packages/Suave/lib/net40/Suave.dll"
#I "packages/FSharp.Data/lib/net40"
#r "packages/FSharp.Data/lib/net40/FSharp.Data.dll"

open FSharp.Data
open Suave                 // always open suave
open Suave.Http.Successful // for OK-result
open Suave.Web             // for config
open Suave.Types             

printfn "initializing script..."

type Species = HtmlProvider<"http://en.wikipedia.org/wiki/The_world's_100_most_threatened_species">

let species = 
    [ for x in Species.GetSample().Tables.``Species list``.Rows -> 
        x.Type, x.``Common name`` ]

let speciesSorted = 
    species 
      |> Seq.countBy fst 
      |> Seq.sortBy (snd >> (~-))
      |> Seq.toList

let config = 
    let port = System.Environment.GetEnvironmentVariable("PORT")
    { defaultConfig with 
        logger = Logging.Loggers.saneDefaultsFor Logging.LogLevel.Verbose
        bindings=[ (if port = null then HttpBinding.mk' HTTP  "localhost" 8080
                    else HttpBinding.mk' HTTP  "0.0.0.0" (int32 port)) ] }

let text = 
    [ yield "<html><body><ul>"
      for (category,count) in speciesSorted do
         yield sprintf "<li>Category <b>%s</b>: <b>%d</b></li>" category count 
      yield "</ul></body></html>" ]
    |> String.concat "\n"

let angularHeader = """<head>
<link rel="stylesheet" href="https://maxcdn.bootstrapcdn.com/bootstrap/3.2.0/css/bootstrap.min.css">
<script src="https://ajax.googleapis.com/ajax/libs/angularjs/1.2.26/angular.min.js"></script>
</head>"""

let fancyText = 
    [ yield """<html>"""
      yield angularHeader
      yield """ <body>"""
      yield """  <table class="table table-striped">"""
      yield """   <thead><tr><th>Category</th><th>Count</th></tr></thead>"""
      yield """   <tbody>"""
      for (category,count) in speciesSorted do
         yield sprintf "<tr><td>%s</td><td>%d</td></tr>" category count 
      yield """   </tbody>"""
      yield """  </table>"""
      yield """ </body>""" 
      yield """</html>""" ]
    |> String.concat "\n"

printfn "starting web server..."

startWebServer config (OK fancyText)
printfn "exiting server..."


