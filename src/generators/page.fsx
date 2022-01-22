#r "../_lib/Fornax.Core.dll"
#load "layout.fsx"

open Html


let generate' (ctx : SiteContents) (pageName: string) =
    let page =
        ctx.TryGetValues<Contentblockloader.Page> ()
        |> Option.defaultValue Seq.empty
        |> Seq.find (fun n -> n.file = pageName)

    let siteInfo = ctx.TryGetValue<Globalloader.SiteInfo> ()
    let desc =
        siteInfo
        |> Option.map (fun si -> si.description)
        |> Option.defaultValue ""

    Layout.layout ctx page.title [
    section [Class "hero is-info is-medium is-bold"] [
      div [Class "hero-body"] [
        div [Class "container has-text-centered"] [
          h1 [Class "title"] [!!desc]
        ]
      ]
    ]
    div [Class "container"] [
      section [Class "articles"] [
        div [Class "column is-8 is-offset-2"] [
            div [Class "card article"] [
                div [Class "card-content"] [
                    div [Class "content article-body"] [
                        !! page.content
                    ]
                ]
            ]
        ]
      ]
    ]]

let generate (ctx : SiteContents) (projectRoot: string) (page: string) =
    generate' ctx page
    |> Layout.render ctx
