#r "../_lib/Fornax.Core.dll"
#load "postloader.fsx"

type Page = {
    title: string
    link: string
    position: int
    file: string
    content: string
}

[<Literal>]
let indexPage = "pages/index.md"

let isIndexPage (p:Postloader.Post) = p.file = indexPage

let loader (projectRoot: string) (siteContent: SiteContents) =

    Postloader.loader' "pages" projectRoot siteContent
    |> Array.map (fun post ->
        { title = post.title
          link = if isIndexPage post then "/" else post.link
          position = post.position |> Option.defaultValue System.Int32.MaxValue
          file = post.file
          content = post.content })
    |> Array.indexed
    |> Array.sortBy (fun (origOrder,page) -> (page.position,origOrder))
    |> Array.iter (snd >> siteContent.Add)

    siteContent
