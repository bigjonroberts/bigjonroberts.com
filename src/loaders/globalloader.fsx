#r "../_lib/Fornax.Core.dll"

type SiteInfo = {
    title: string
    description: string
    postPageSize: int
}

let loader (projectRoot: string) (siteContent: SiteContents) =
    let siteInfo =
        { title = "Big Jon Roberts";
          description = "The man, the myth"
          postPageSize = 5 }
    siteContent.Add(siteInfo)

    siteContent
