﻿namespace Kl.Tests

open NUnit.Framework
open Kl
open Kl.Startup
open TestCommon

[<TestFixture>]
type SymbolEvaluationTests() =

    [<Test>]
    member this.``symbols not starting with an uppercase letter and not at the head of an application are always idle``() =
        assertEq (Sym "abc") (runIt "(let Id (lambda X X) abc)")
        assertEq (Sym "if") (runIt "(let Id (lambda X X) if)")
        assertEq (Sym "+") (runIt "(let Id (lambda X X) +)")

    [<Test>]
    member this.``symbols starting with an uppercase letter not at the head of an application are idle if not in local scope``() =
        assertEq (Sym "ABC") (runIt "(let Id (lambda X X) ABC)")

    [<Test>]
    member this.``result of interning a string is equal to symbol with name that is equal to that string``() =
        assertEq (Sym "hi") (runIt "(intern \"hi\")")

    [<Test>]
    member this.``interned symbols can contain any characters``() =
        assertEq (Sym "@!#$") (runIt "(intern \"@!#$\")")
        assertEq (Sym "(),[];{}") (runIt "(intern \"(),[];{}\")")
        assertEq (Sym "   ") (runIt "(intern \"   \")") // space space space
        
    [<Test>]
    member this.``both symbols starting with or with-out an uppercase letter or non-letter can be idle``() =
        assertEq
            (Cons(Sym "A", Cons(Sym "-->", Cons(Sym "boolean", Empty))))
            (runIt "(cons A (cons --> (cons boolean ())))")

    [<Test>]
    member this.``a function defun'd with an upper-case name will not get resolved when applied``() =
        // because the symbol will be resolved using
        // only the local namespace
        let env = baseEnv()
        runInEnv env "(defun Inc (X) (+ X 1))" |> ignore
        assertErrorInEnv env "(Inc 5)"

    [<Test>]
    member this.``setting a global symbol to a lambda will not allow it to be used as a defun``() =
        // because lower-case named functions only get resolved
        // using the function namespace, not the symbol namespace
        let env = baseEnv()
        runInEnv env "(set inc (lambda X (+ X 1)))" |> ignore
        assertErrorInEnv env "(inc 5)"
