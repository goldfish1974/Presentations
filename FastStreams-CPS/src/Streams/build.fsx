// --------------------------------------------------------------------------------------
// FAKE build script 
// --------------------------------------------------------------------------------------

#I "packages/FAKE/tools"
#r "packages/FAKE/tools/FakeLib.dll"
//#load "packages/SourceLink.Fake/tools/SourceLink.fsx"
open System
open System.IO
open Fake 
open Fake.Git
open Fake.ReleaseNotesHelper
open Fake.AssemblyInfoFile
//open SourceLink

// --------------------------------------------------------------------------------------
// Information about the project to be used at NuGet and in AssemblyInfo files
// --------------------------------------------------------------------------------------

let project = "Streams"
let authors = ["Nessos Information Technologies, Nick Palladinos, Kostas Rontogiannis"]
let summary = "A lightweight F#/C# library for efficient functional-style pipelines on streams of data."

let description = """
    A lightweight F#/C# library for efficient functional-style pipelines on streams of data.
"""

let tags = "F#/C# Streams"

let gitHome = "https://github.com/nessos"
let gitName = "Streams"
let gitRaw = environVarOrDefault "gitRaw" "https://raw.github.com/nessos"

let testAssemblies = 
    [
        yield "bin/Streams.Tests.exe"
        yield "bin/Streams.Tests.CSharp.exe"
    ]

//
//// --------------------------------------------------------------------------------------
//// The rest of the code is standard F# build script 
//// --------------------------------------------------------------------------------------

//// Read release notes & version info from RELEASE_NOTES.md
Environment.CurrentDirectory <- __SOURCE_DIRECTORY__

module Streams =
    let release = parseReleaseNotes (IO.File.ReadAllLines "RELEASE_NOTES.md")
    let nugetVersion = release.NugetVersion

Target "BuildVersion" (fun _ ->
    Shell.Exec("appveyor", sprintf "UpdateBuild -Version \"%s\"" Streams.nugetVersion) |> ignore
)

// Generate assembly info files with the right version & up-to-date information
Target "AssemblyInfo" (fun _ ->
    let attributes version =
        [ 
            Attribute.Title project
            Attribute.Product project
            Attribute.Company "Nessos Information Technologies"
            Attribute.Version version
            Attribute.FileVersion version
        ]

    CreateFSharpAssemblyInfo "src/Streams.Core/AssemblyInfo.fs" <| attributes Streams.release.AssemblyVersion
    CreateCSharpAssemblyInfo "src/Streams.CSharp/Properties/AssemblyInfo.cs" <| attributes Streams.release.AssemblyVersion
)


// --------------------------------------------------------------------------------------
// Clean build results & restore NuGet packages

Target "RestorePackages" (fun _ ->
    !! "./**/packages.config"
    |> Seq.iter (RestorePackage (fun p -> { p with ToolPath = "./.nuget/NuGet.exe" }))
)

Target "Clean" (fun _ ->
    CleanDirs (!! "**/bin/Release/")
    CleanDirs (!! "**/bin/Debug/")
    CleanDir "bin/"
)

//
//// --------------------------------------------------------------------------------------
//// Build library & test project

let configuration = environVarOrDefault "Configuration" "Release"

Target "Build" (fun _ ->
    // Build the rest of the project
    { BaseDirectory = __SOURCE_DIRECTORY__
      Includes = [ project + ".sln" ]
      Excludes = [] } 
    |> MSBuild "" "Build" ["Configuration", configuration]
    |> Log "AppBuild-Output: "
)


// --------------------------------------------------------------------------------------
// Run the unit tests using test runner & kill test runner when complete

Target "RunTests" (fun _ ->
    let nunitVersion = GetPackageVersion "packages" "NUnit.Runners"
    let nunitPath = sprintf "packages/NUnit.Runners.%s/tools" nunitVersion
    ActivateFinalTarget "CloseTestRunner"

    testAssemblies
    |> NUnit (fun p ->
        { p with
            Framework = "v4.0.30319"
            ToolPath = nunitPath
            DisableShadowCopy = true
            TimeOut = TimeSpan.FromMinutes 20.
            OutputFile = "TestResults.xml" })
)

FinalTarget "CloseTestRunner" (fun _ ->  
    ProcessHelper.killProcess "nunit-agent.exe"
)
//
//// --------------------------------------------------------------------------------------
//// Build a NuGet package

let addFile (target : string) (file : string) =
    if File.Exists (Path.Combine("nuget", file)) then (file, Some target, None)
    else raise <| new FileNotFoundException(file)

let addAssembly (target : string) assembly =
    let includeFile force file =
        let file = file
        if File.Exists (Path.Combine("nuget", file)) then [(file, Some target, None)]
        elif force then raise <| new FileNotFoundException(file)
        else []

    seq {
        yield! includeFile true assembly
        yield! includeFile false <| Path.ChangeExtension(assembly, "pdb")
        yield! includeFile false <| Path.ChangeExtension(assembly, "xml")
        yield! includeFile false <| assembly + ".config"
    }

Target "NuGet" (fun _ ->
    let nugetPath = ".nuget/NuGet.exe"

    let description = description.Replace("\r", "").Replace("\n", "").Replace("  ", " ")
    
    NuGet (fun p -> 
        { p with   
            Authors = authors
            Project = "Streams"
            Summary = summary
            Description = description
            Version = Streams.nugetVersion
            ReleaseNotes = String.concat " " Streams.release.Notes
            Tags = tags
            OutputPath = "bin"
            ToolPath = nugetPath
            AccessKey = getBuildParamOrDefault "nugetkey" ""
            Files =
                [
                    yield! addAssembly @"lib\net45" @"..\bin\Streams.Core.dll"
                ]
            Publish = hasBuildParam "nugetkey" })
        ("nuget/Streams.nuspec")

    NuGet (fun p -> 
        { p with   
            Authors = authors
            Project = "Streams.CSharp"
            Summary = summary
            Description = description
            Version = Streams.nugetVersion
            ReleaseNotes = String.concat " " Streams.release.Notes
            Tags = tags
            OutputPath = "bin"
            Dependencies = [ "Streams", RequireExactly Streams.nugetVersion ]
            ToolPath = nugetPath
            Files =
                [
                    yield! addAssembly @"lib\net45" @"..\bin\Streams.CSharp.dll"
                ]
            AccessKey = getBuildParamOrDefault "nugetkey" ""
            Publish = hasBuildParam "nugetkey" })
        ("nuget/Streams.nuspec")
)

Target "GenerateDocs" (fun _ ->
    executeFSIWithArgs "docs/tools" "generate.fsx" ["--define:RELEASE"] [] |> ignore
)

Target "ReleaseDocs" (fun _ ->
    let tempDocsDir = "temp/gh-pages"
    CleanDir tempDocsDir
    Repository.cloneSingleBranch "" (gitHome + "/" + gitName + ".git") "gh-pages" tempDocsDir

    fullclean tempDocsDir
    CopyRecursive "docs/output" tempDocsDir true |> tracefn "%A"
    StageAll tempDocsDir
    Commit tempDocsDir (sprintf "Update generated documentation for Streams %s" Streams.release.NugetVersion)
    Branches.push tempDocsDir
)


Target "Release" DoNothing

// --------------------------------------------------------------------------------------
// Run all targets by default. Invoke 'build <Target>' to override

Target "Prepare" DoNothing
Target "PrepareRelease" DoNothing
Target "Default" DoNothing
Target "Help" (fun _ -> PrintTargets())

"Clean"
  ==> "RestorePackages"
  ==> "AssemblyInfo"
  ==> "Prepare"
  ==> "Build"
  ==> "RunTests"
  ==> "Default"

"Build"
  ==> "PrepareRelease" 
  ==> "NuGet"
  ==> "GenerateDocs"
  ==> "ReleaseDocs"
  ==> "Release"

RunTargetOrDefault "Default"