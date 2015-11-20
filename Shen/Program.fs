﻿open Kl

[<EntryPoint>]
let main args =
    let stopwatch = System.Diagnostics.Stopwatch.StartNew()
    let files = [
                    "toplevel.kl"
                    "core.kl"
                    "sys.kl"
                    "sequent.kl"
                    "yacc.kl"
                    "reader.kl"
                    "prolog.kl"
                    "track.kl"
                    "load.kl"
                    "writer.kl"
                    "macros.kl"
                    "declarations.kl"
                    "t-star.kl" // TODO contrary to spec, this gets loaded before types.kl?
                                // it contains (defun shen.typecheck ...) which types.kl uses
                                // Double check this now that irresolvable symbols are Errors instead of failures
                    "types.kl"
                ]
    let klFolder = @"..\..\..\KLambda"
    let rec astToStr = function
        | ComboToken tokens -> sprintf "(%s)" <| String.concat " " (Seq.map astToStr tokens)
        | BoolToken b -> if b then "true" else "false"
        | NumberToken n -> n.ToString()
        | StringToken s -> "\"" + s + "\""
        | SymbolToken s -> s
    let env = KlBuiltins.baseEnv ()
    for file in (List.map (fun f -> System.IO.Path.Combine(klFolder, f)) files) do
        printfn ""
        printfn "Loading %s" file
        printfn ""
        stdout.Flush()
        let text = System.IO.File.ReadAllText(file)
        for ast in KlTokenizer.tokenizeAll text do
            match ast with
            | ComboToken (command :: symbol :: _) ->
                printfn "%s %s" (astToStr command) (astToStr symbol)
                let expr = KlParser.parse Head ast
                KlEvaluator.eval env expr |> ignore
            | _ -> () // ignore copyright block at top
    let testDir = System.IO.Path.Combine(System.Environment.CurrentDirectory, "..\\..\\..\\Tests")
    let readmePath = System.IO.Path.Combine(testDir, "README.shen")
    let testsPath = System.IO.Path.Combine(testDir, "tests.shen")
    let runIt = KlTokenizer.tokenize >> KlParser.parse Head >> KlEvaluator.eval env >> ignore
    printfn ""
    printfn "Loading done"
    printfn "Time: %s" <| stopwatch.Elapsed.ToString()
    printfn ""
//    printfn "Starting shen repl..."
//    printfn ""
//    while true do
//        printf "> "
//        let line = System.Console.ReadLine()
//        match line |> KlTokenizer.tokenize |> KlParser.parse Head |> KlEvaluator.eval env with
//        | ValueResult v -> printfn "%s" (KlBuiltins.klStr v)
//        | ErrorResult e -> printfn "ERROR %s" e
//    KlEvaluator.eval env (AppExpr (Head, SymbolExpr "shen.shen", [])) |> ignore
    KlEvaluator.eval env (AppExpr (Head, (SymbolExpr "load"), [StringExpr readmePath])) |> ignore
    KlEvaluator.eval env (AppExpr (Head, (SymbolExpr "load"), [StringExpr testsPath])) |> ignore
    printfn ""
    printfn "Press any key to exit..."
    System.Console.ReadKey() |> ignore
    0