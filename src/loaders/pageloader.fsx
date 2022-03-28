#r "../_lib/Fornax.Core.dll"
#r "../_lib/Markdig.dll"

open System.IO
open Markdig

type Page = {
    title: string
    link: string
    position: int
    file: string
    content: string
}

let markdownPipeline =
    MarkdownPipelineBuilder()
        .UsePipeTables()
        .UseGridTables()
        .Build()

let isSeparator (input : string) =
    input.StartsWith "---"

let isSummarySeparator (input: string) =
    input.Contains "<!--more-->"


///`fileContent` - content of page to parse. Usually whole content of `.md` file
///returns content of config that should be used for the page
let getConfig (fileContent : string) =
    let fileContent = fileContent.Split '\n'
    let fileContent = fileContent |> Array.skip 1 //First line must be ---
    let indexOfSeperator = fileContent |> Array.findIndex isSeparator
    let splitKey (line: string) =
        let seperatorIndex = line.IndexOf(':')
        if seperatorIndex > 0 then
            let key = line.[.. seperatorIndex - 1].Trim().ToLower()
            let value = line.[seperatorIndex + 1 ..].Trim()
            Some(key, value)
        else
            None
    fileContent
    |> Array.splitAt indexOfSeperator
    |> fst
    |> Seq.choose splitKey
    |> Map.ofSeq

///`fileContent` - content of page to parse. Usually whole content of `.md` file
///returns HTML version of content of the page
let getContent (fileContent : string) =
    let fileContent = fileContent.Split '\n'
    let fileContent = fileContent |> Array.skip 1 //First line must be ---
    let indexOfSeperator = fileContent |> Array.findIndex isSeparator
    let _, content = fileContent |> Array.splitAt indexOfSeperator

    let summary, content =
        match content |> Array.tryFindIndex isSummarySeparator with
        | Some indexOfSummary ->
            let summary, _ = content |> Array.splitAt indexOfSummary
            summary, content
        | None ->
            content, content

    let summary = summary |> Array.skip 1 |> String.concat "\n"
    let content = content |> Array.skip 1 |> String.concat "\n"

    Markdown.ToHtml(summary, markdownPipeline),
    Markdown.ToHtml(content, markdownPipeline)

let trimString (str : string) =
    str.Trim().TrimEnd('"').TrimStart('"')

[<Literal>]
let indexPage = "pages/index.html"

let isIndexPage (link: string) = link = indexPage

let private loadFile (rootDir: string) (n: string) =
    let text = File.ReadAllText n

    let config = getConfig text
    let content = getContent text |> snd

    let chopLength =
        if rootDir.EndsWith(Path.DirectorySeparatorChar) then rootDir.Length
        else rootDir.Length + 1

    let dirPart =
        n
        |> Path.GetDirectoryName
        |> fun x -> x.[chopLength .. ]

    let file = Path.Combine(dirPart, (n |> Path.GetFileNameWithoutExtension) + ".md").Replace("\\", "/")
    let link = "/" + Path.Combine(dirPart, (n |> Path.GetFileNameWithoutExtension) + ".html").Replace("\\", "/")

    let title = config |> Map.find "title" |> trimString
    let position =
        Map.tryFind "position" config
        |> Option.map (trimString >> System.Int32.Parse)
        |> Option.defaultValue System.Int32.MaxValue

    let tags =
        let tagsOpt =
            config
            |> Map.tryFind "tags"
            |> Option.map (trimString >> fun n -> n.Split ',' |> Array.toList)
        defaultArg tagsOpt []

    { file = file
      link = if isIndexPage link then "/" else link
      title = title
      position = position
      content = content }

let private loader' (projectRoot: string) =
    let contentPath = Path.Combine(projectRoot, "pages")
    let options = EnumerationOptions(RecurseSubdirectories = true)
    let files = Directory.GetFiles(contentPath, "*", options)
    files
    |> Array.filter (fun n -> n.EndsWith ".md")
    |> Array.map (loadFile projectRoot)
    |> Array.indexed
    |> Array.sortBy (fun (origOrder,page) -> (page.position,origOrder))
    |> Array.map snd


let loader (projectRoot: string) (siteContent: SiteContents) =
    loader' projectRoot
    |> Seq.iter siteContent.Add

    siteContent
