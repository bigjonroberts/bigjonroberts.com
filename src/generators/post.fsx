#r "../_lib/Fornax.Core.dll"
#load "layout.fsx"

open Html


let generate' (ctx : SiteContents) (page: string) =
    let post =
        ctx.TryGetValues<Postloader.Post> ()
        |> Option.defaultValue Seq.empty
        |> Seq.tryFind (fun n ->
            printfn "checking if '%s' matches '%s'" n.file page
            n.file = page)

    match post with
    | Some post ->
        let siteInfo = ctx.TryGetValue<Globalloader.SiteInfo> ()
        let desc =
            siteInfo
            |> Option.map (fun si -> si.description)
            |> Option.defaultValue ""

        Layout.layout ctx post.title [
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
                        Layout.postLayout false post
                    ]
                ]
            ]
        ]
    | None ->
        printfn "page '%s' not found" page
        html [] [ !! "page not found" ]



let generate (ctx : SiteContents) (projectRoot: string) (page: string) =
    generate' ctx page
    |> Layout.render ctx