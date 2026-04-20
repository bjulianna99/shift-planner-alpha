namespace DUE_FSharp_SPASandbox_2026

open System
open WebSharper
open WebSharper.UI
open WebSharper.UI.Html
open WebSharper.UI.Client
open WebSharper.UI.Templating

[<JavaScript>]
module Client =

    type IndexTemplate = Template<"wwwroot/index.html", ClientLoad.FromDocument>

    type ShiftType =
        | Day
        | Night
        | Rest
        | Leave

    type ShiftEntry = {
        Date: DateTime
        ShiftType: ShiftType
        Note: string
    }

    let shiftTypeToText shiftType =
        match shiftType with
        | Day -> "Day shift"
        | Night -> "Night shift"
        | Rest -> "Rest day"
        | Leave -> "Leave"

    let shiftTypeBadgeClass shiftType =
        match shiftType with
        | Day -> "bg-blue-100 text-blue-700"
        | Night -> "bg-purple-100 text-purple-700"
        | Rest -> "bg-green-100 text-green-700"
        | Leave -> "bg-yellow-100 text-yellow-700"

    let sampleShifts = [
        {
            Date = DateTime(2026, 4, 20)
            ShiftType = Day
            Note = "Regular day shift"
        }
        {
            Date = DateTime(2026, 4, 21)
            ShiftType = Night
            Note = "Night duty"
        }
        {
            Date = DateTime(2026, 4, 22)
            ShiftType = Rest
            Note = "Scheduled rest day"
        }
        {
            Date = DateTime(2026, 4, 23)
            ShiftType = Day
            Note = "Morning shift"
        }
        {
            Date = DateTime(2026, 4, 24)
            ShiftType = Leave
            Note = "Personal leave"
        }
    ]

    let shiftsVar = Var.Create sampleShifts

    let dateInputVar = Var.Create ""
    let noteInputVar = Var.Create ""
    let selectedShiftTypeVar = Var.Create Day
    let formMessageVar = Var.Create ""

    let removeShift entry =
        let updated =
            shiftsVar.Value
            |> List.filter (fun x ->
                not (
                    x.Date = entry.Date &&
                    x.ShiftType = entry.ShiftType &&
                    x.Note = entry.Note
                )
            )

        shiftsVar.Set updated

    let addShift () =
        let rawDate = dateInputVar.Value.Trim()
        let rawNote = noteInputVar.Value.Trim()

        match DateTime.TryParse(rawDate) with
        | true, parsedDate ->
            if rawNote = "" then
                formMessageVar.Set "Please enter a note."
            else
                let newEntry = {
                    Date = parsedDate
                    ShiftType = selectedShiftTypeVar.Value
                    Note = rawNote
                }

                shiftsVar.Set (shiftsVar.Value @ [ newEntry ])
                dateInputVar.Set ""
                noteInputVar.Set ""
                selectedShiftTypeVar.Set Day
                formMessageVar.Set "Shift entry added successfully."
        | false, _ ->
            formMessageVar.Set "Invalid date format. Use yyyy-MM-dd."

    let renderShiftEntry entry =
        div [attr.``class`` "bg-white rounded-xl shadow-sm border border-gray-200 p-4 mb-4"] [
            div [attr.``class`` "flex flex-col md:flex-row md:items-center md:justify-between gap-3"] [
                div [] [
                    h3 [attr.``class`` "text-lg font-semibold text-gray-800"] [
                        text (entry.Date.ToString("yyyy-MM-dd"))
                    ]
                    p [attr.``class`` "text-gray-600 mt-1"] [
                        text entry.Note
                    ]
                ]

                div [attr.``class`` "flex items-center gap-3 flex-wrap"] [
                    span [attr.``class`` ("inline-block px-3 py-1 rounded-full text-sm font-medium " + shiftTypeBadgeClass entry.ShiftType)] [
                        text (shiftTypeToText entry.ShiftType)
                    ]

                    button [
                        attr.``class`` "px-3 py-1 rounded-lg bg-red-500 hover:bg-red-600 text-white text-sm font-medium transition"
                        on.click (fun _ _ -> removeShift entry)
                    ] [
                        text "Delete"
                    ]
                ]
            ]
        ]

    let shiftTypeSelectorButton shiftType labelText =
        button [
            attr.``type`` "button"
            attr.``class`` "px-3 py-2 rounded-lg border text-sm font-medium transition bg-white text-gray-700 border-gray-300 hover:bg-gray-50"
            on.click (fun _ _ -> selectedShiftTypeVar.Set shiftType)
        ] [
            text labelText
        ]

    let statsCards () =
        Doc.BindView (fun (shifts: ShiftEntry list) ->
            let totalCount = List.length shifts
            let dayCount = shifts |> List.filter (fun x -> x.ShiftType = Day) |> List.length
            let nightCount = shifts |> List.filter (fun x -> x.ShiftType = Night) |> List.length
            let restLeaveCount = shifts |> List.filter (fun x -> (x.ShiftType = Rest) || (x.ShiftType = Leave)) |> List.length

            section [attr.``class`` "grid md:grid-cols-4 gap-4"] [
                div [attr.``class`` "bg-blue-50 border border-blue-200 rounded-xl p-4"] [
                    h2 [attr.``class`` "text-lg font-semibold text-blue-800 mb-2"] [
                        text "Current stage"
                    ]
                    p [attr.``class`` "text-blue-700"] [
                        text "Interactive project version"
                    ]
                ]

                div [attr.``class`` "bg-white border border-gray-200 rounded-xl p-4"] [
                    h2 [attr.``class`` "text-lg font-semibold text-gray-800 mb-2"] [
                        text "Entries"
                    ]
                    p [attr.``class`` "text-gray-600"] [
                        text (string totalCount + " shift entries")
                    ]
                ]

                div [attr.``class`` "bg-white border border-gray-200 rounded-xl p-4"] [
                    h2 [attr.``class`` "text-lg font-semibold text-gray-800 mb-2"] [
                        text "Day / Night"
                    ]
                    p [attr.``class`` "text-gray-600"] [
                        text ("Day: " + string dayCount + " | Night: " + string nightCount)
                    ]
                ]

                div [attr.``class`` "bg-white border border-gray-200 rounded-xl p-4"] [
                    h2 [attr.``class`` "text-lg font-semibold text-gray-800 mb-2"] [
                        text "Rest / Leave"
                    ]
                    p [attr.``class`` "text-gray-600"] [
                        text (string restLeaveCount + " entries")
                    ]
                ]
            ]
        ) shiftsVar.View

    let shiftList () =
        Doc.BindView (fun (shifts: ShiftEntry list) ->
            div [] (
                shifts
                |> List.sortBy (fun x -> x.Date)
                |> List.map renderShiftEntry
            )
        ) shiftsVar.View

    let formMessage () =
        Doc.BindView (fun messageText ->
            if messageText = "" then
                Doc.Empty
            else
                p [attr.``class`` "text-sm text-gray-600 mt-3"] [
                    text messageText
                ]
        ) formMessageVar.View

    let pageContent =
        div [attr.``class`` "space-y-8"] [

            section [attr.id "home"] [
                div [attr.``class`` "bg-white rounded-2xl shadow-sm border border-gray-200 p-6 md:p-8"] [
                    h1 [attr.``class`` "text-3xl md:text-4xl font-bold text-gray-900 mb-4"] [
                        text "Shift Planner Alpha"
                    ]

                    p [attr.``class`` "text-lg text-gray-700 mb-4"] [
                        text "A simple F# WebSharper application for managing and viewing shift schedules."
                    ]

                    p [attr.``class`` "text-gray-600"] [
                        text "This version already supports adding and deleting shift entries in an interactive interface."
                    ]
                ]
            ]

            statsCards ()

            section [attr.id "add-shift"] [
                div [attr.``class`` "bg-white rounded-2xl shadow-sm border border-gray-200 p-6"] [
                    h2 [attr.``class`` "text-2xl font-semibold text-gray-800 mb-4"] [
                        text "Add new shift"
                    ]

                    div [attr.``class`` "grid md:grid-cols-2 gap-4"] [
                        div [] [
                            label [attr.``class`` "block text-sm font-medium text-gray-700 mb-2"] [
                                text "Date"
                            ]
                            Doc.InputType.Text [
                                attr.``class`` "w-full px-4 py-2 rounded-lg border border-gray-300"
                                attr.placeholder "yyyy-MM-dd"
                            ] dateInputVar
                        ]

                        div [] [
                            label [attr.``class`` "block text-sm font-medium text-gray-700 mb-2"] [
                                text "Note"
                            ]
                            Doc.InputType.Text [
                                attr.``class`` "w-full px-4 py-2 rounded-lg border border-gray-300"
                                attr.placeholder "Enter shift note"
                            ] noteInputVar
                        ]
                    ]

                    div [attr.``class`` "mt-4"] [
                        label [attr.``class`` "block text-sm font-medium text-gray-700 mb-2"] [
                            text "Shift type"
                        ]

                        div [attr.``class`` "flex flex-wrap gap-2"] [
                            shiftTypeSelectorButton Day "Day shift"
                            shiftTypeSelectorButton Night "Night shift"
                            shiftTypeSelectorButton Rest "Rest day"
                            shiftTypeSelectorButton Leave "Leave"
                        ]
                    ]

                    div [attr.``class`` "mt-4"] [
                        button [
                            attr.``type`` "button"
                            attr.``class`` "px-5 py-2 rounded-lg bg-blue-600 text-white font-medium"
                            on.click (fun _ _ -> addShift ())
                        ] [
                            text "Add shift"
                        ]
                    ]

                    formMessage ()
                ]
            ]

            section [attr.id "sample-shifts"] [
                div [attr.``class`` "flex items-center justify-between mb-4"] [
                    h2 [attr.``class`` "text-2xl font-semibold text-gray-800"] [
                        text "Shift entries"
                    ]
                ]

                shiftList ()
            ]

            section [attr.id "about-project"] [
                div [attr.``class`` "bg-white rounded-2xl shadow-sm border border-gray-200 p-6"] [
                    h2 [attr.``class`` "text-2xl font-semibold text-gray-800 mb-3"] [
                        text "About this project"
                    ]

                    p [attr.``class`` "text-gray-700 mb-3"] [
                        text "Shift Planner Alpha is being developed step by step as a semester project."
                    ]

                    p [attr.``class`` "text-gray-600"] [
                        text "Later versions can include sorting, filtering, summaries, and workload analysis."
                    ]
                ]
            ]
        ]

    [<SPAEntryPoint>]
    let Main () =
        IndexTemplate()
            .Content(pageContent)
            .Bind()