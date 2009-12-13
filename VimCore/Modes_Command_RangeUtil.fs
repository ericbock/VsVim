﻿#light

namespace VimCore.Modes.Command
open VimCore
open Microsoft.VisualStudio.Text

type internal Range =
    /// Valid input range.  SnapshotSpan has the actual range of the text while the KeyInput
    /// list is the remaining input after the range
    | ValidRange of SnapshotSpan * KeyInput list
    /// No range given, original list returned
    | NoRange of KeyInput list
    /// Invalid range.  String contains the error
    | Invalid of string * KeyInput list

type internal ItemRangeKind = 
    | LineNumber
    | CurrentLine

type internal ItemRange = 
    | ValidRange of SnapshotSpan * ItemRangeKind * KeyInput list
    | NoRange
    | Error of string

module internal RangeUtil =

    /// Parse out a number from the input string
    let ParseNumber (input:KeyInput list) =

        // Parse out the input into the list of digits and remaining input
        let rec getDigits (input:KeyInput list) =
            match input |> List.isEmpty with
            | true ->([],input)
            | false -> 
                let head = input |> List.head
                if head.IsDigit then
                    let restDigits,restInput = getDigits (input |> List.tail)
                    (head :: restDigits, restInput)
                else 
                    ([],input)
            
        let digits,remaining = getDigits input
        let numberStr = 
            digits 
                |> Seq.ofList
                |> Seq.map (fun x -> x.Char)
                |> Array.ofSeq
                |> StringUtil.OfCharArray
        let mutable number = 0
        match System.Int32.TryParse(numberStr, &number) with
        | false -> (None,input)
        | true -> (Some(number), remaining)

    /// Parse out a line number 
    let ParseLineNumber (tss:ITextSnapshot) (input:KeyInput list) =
    
        let msg = "Invalid Range: Could not find a valid number"
        let opt,remaining = ParseNumber input
        match opt with 
        | Some(number) ->
            let number = TssUtil.VimLineToTssLine number
            if number < tss.LineCount then 
                let line = tss.GetLineFromLineNumber(number)
                ValidRange(line.ExtentIncludingLineBreak, LineNumber, remaining)
            else
                let msg = sprintf "Invalid Range: Line Number %d is not a valid number in the file" number
                Error(msg)
        | None -> Error("Expected a line number")


    /// Parse out a single item in the range.
    let ParseItem (point:SnapshotPoint) (map:MarkMap) (list:KeyInput list) =
        let head = list |> List.head 
        if head.IsDigit then
            ParseLineNumber point.Snapshot list
        else if head.Char = '.' then
            let span = point.GetContainingLine().ExtentIncludingLineBreak
            ValidRange(span, CurrentLine, list |> List.tail)
        else
            NoRange

    let ParseRangeCore (point:SnapshotPoint) (map:MarkMap) (originalInput:KeyInput list) =

        let parseRight (point:SnapshotPoint) map (leftSpan:SnapshotSpan) remainingInput : Range = 
            let right = ParseItem point map remainingInput 
            match right with 
            | NoRange -> Invalid("Invalid Range: Right hand side is missing", originalInput)
            | Error(msg) -> Invalid(msg, originalInput)
            | ValidRange(rightSpan,_,remainingInput) ->
                let fullSpan = new SnapshotSpan(leftSpan.Start,rightSpan.End)
                Range.ValidRange(fullSpan, remainingInput)
        
        // Parse out the separator 
        let parseWithLeft (leftSpan:SnapshotSpan) (remainingInput:KeyInput list) kind =
            match remainingInput |> List.isEmpty with
            | true ->  if kind = CurrentLine then Range.ValidRange(leftSpan,remainingInput) else Range.NoRange(originalInput)
            | false -> 
                let head = remainingInput |> List.head 
                let remainingInput = remainingInput |> List.tail
                if head.Char = ',' then parseRight point map leftSpan remainingInput
                else if head.Char = ';' then 
                    let point = leftSpan.End.GetContainingLine().Start
                    parseRight point map leftSpan remainingInput
                else if kind = LineNumber then
                    Range.NoRange(originalInput)
                else if kind = CurrentLine then
                    Range.ValidRange(leftSpan, remainingInput)
                else
                    let msg = "Invalid Range: Expected , or ;"
                    Invalid(msg,originalInput)
        
        let left = ParseItem point map originalInput
        match left with 
        | NoRange -> Range.NoRange(originalInput)
        | Error(_) -> Range.NoRange(originalInput)
        | ValidRange(leftSpan,kind,range) -> parseWithLeft leftSpan range kind


    let ParseRange (point:SnapshotPoint) (map:MarkMap) (list:KeyInput list) = 
        match list |> List.isEmpty with
        | true -> Range.NoRange(list)
        | false ->
            let head = list |> List.head
            if head.Char = '%' then 
                let tss = point.Snapshot
                let span = new SnapshotSpan(tss, 0, tss.Length)
                Range.ValidRange(span, List.tail list)
            else
                ParseRangeCore point map list