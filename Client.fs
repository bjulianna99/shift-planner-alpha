
namespace DUE_FSharp_SPASandbox_2026

open System
open WebSharper
open WebSharper.JavaScript
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

    type ShiftFilter =
        | All
        | OnlyDay
        | OnlyNight
        | OnlyRest
        | OnlyLeave

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

    let shiftFilterToText shiftFilter =
        match shiftFilter with
        | All -> "All"
        | OnlyDay -> "Day"
        | OnlyNight -> "Night"
        | OnlyRest -> "Rest"
        | OnlyLeave -> "Leave"

    let shiftTypeBadgeClass shiftType =
        match shiftType with
        | Day -> "bg-blue-100 text-blue-700"
        | Night -> "bg-purple-100 text-purple-700"
        | Rest -> "bg-green-100 text-green-700"
        | Leave -> "bg-yellow-100 text-yellow-700"

    let getDateStatus (date: DateTime) =
        let today = DateTime.Today

        if date.Date < today then
            "Completed"
        elif date.Date = today then
            "Today"
        else
            "Upcoming"

    let dateStatusBadgeClass (date:DateTime) =
        let today = DateTime.Today

        if date.Date < today then
            "bg-gray-200 text-gray-700"
        elif date.Date = today then
            "bg-orange-100 text-orange-700"
        else 
            "bg-teal-100 text-teal-700"

    let getShiftStatus (date: DateTime) =
        let today = DateTime.Today

        if date.Date = today then
            "Today"
        elif date.Date < today then
            "Past"
        else
            "Upcoming"

    let shiftStatusBadgeClass (date: DateTime) =
        let today = DateTime.Today

        if date.Date = today then
            "bg-orange-100 text-orange-700"
        elif date.Date < today then 
            "bg-gray-200 text-gray-700"
        else 
            "bg-teal-100 text-teal-700"

    let filterShifts filter shifts =
        match filter with
        | All -> shifts
        | OnlyDay -> shifts |> List.filter (fun x -> x.ShiftType = Day)
        | OnlyNight -> shifts |> List.filter (fun x -> x.ShiftType = Night)
        | OnlyRest -> shifts |> List.filter (fun x -> x.ShiftType = Rest)
        | OnlyLeave -> shifts |> List.filter (fun x -> x.ShiftType = Leave)

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
    let selectedFilterVar = Var.Create All
    let editingShiftVar : Var<option<ShiftEntry>> = Var.Create None

    let removeShift entry =
        let updated =
            shiftsVar.Value
            |> List.filter (fun x -> x <> entry)
        shiftsVar.Set updated

    let addShift () =
        let rawDate = dateInputVar.Value.Trim()
        let rawNote = noteInputVar.Value.Trim()

        match DateTime.TryParse(rawDate) with
        | true, parsedDate ->
            if rawNote = "" then
                formMessageVar.Set "Please enter a note."
            else
                let exists =
                    shiftsVar.Value
                    |> List.exists (fun x -> x.Date = parsedDate && x.Note = rawNote)

                if exists then
                    formMessageVar.Set "This shift already exists."

                    async {
                        do! Async.Sleep 2000
                        formMessageVar.Set ""
                    } |> Async.Start
                else
                    let newEntry = {
                        Date = parsedDate
                        ShiftType = selectedShiftTypeVar.Value
                        Note = rawNote
                    }

                    match editingShiftVar.Value with
                    | Some oldEntry ->
                        let updated =
                            shiftsVar.Value
                            |> List.map (fun x -> if x = oldEntry then newEntry else x)

                        shiftsVar.Set (updated |> List.sortBy (fun x -> x.Date))
                        editingShiftVar.Set None

                    | None ->
                        shiftsVar.Set (
                            shiftsVar.Value
                            @ [ newEntry]
                            |> List.sortBy (fun x -> x.Date)
                        )
                    
                    dateInputVar.Set ""
                    noteInputVar.Set ""
                    selectedShiftTypeVar.Set Day
                    formMessageVar.Set "Shift entry added successfully."

                    async {
                        do! Async.Sleep 2000
                        formMessageVar.Set ""
                    } |> Async.Start
        | false, _ ->
            formMessageVar.Set "Invalid date format. Use yyyy-MM-dd."

    let renderShiftEntry entry =
        let cardClass =
            if entry.Date.Date < DateTime.Today then
                "bg-gray-50 rounded-xl shadow-sm border border-gray-200 p-4 mb-4 opacity-80"
            else
                "bg-white rounded-xl shadow-sm border border-gray-200 p-4 mb-4"

        div [attr.``class`` cardClass] [
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
                    span [attr.``class`` ("inline-block px-3 py-1 rounded-full text-sm font-medium " + shiftStatusBadgeClass entry.Date)] [
                        text (getShiftStatus entry.Date)
                    ]

                    button [
                        attr.``class`` "px-3 py-1 rounded-lg bg-gray-500 hover:bg-gray-600 text-white text-sm font-medium transition"
                        on.click (fun _ _ ->
                            editingShiftVar.Set (Some entry)
                            dateInputVar.Set (entry.Date.ToString("yyyy-MM-dd"))
                            noteInputVar.Set entry.Note
                            selectedShiftTypeVar.Set entry.ShiftType
                        )
                    ] [
                        text "Edit"
                    ]

                    button [
                        attr.``class`` "px-3 py-1 rounded-lg bg-red-500 hover:bg-red-600 text-white text-sm font-medium transition"
                        on.click (fun _ _ ->
                            let confirmed = JS.Window.Confirm("Are you sure you want to delete this shift entry?")

                            if confirmed then (
                                removeShift entry
                                formMessageVar.Set "Shift deleted"

                                async {
                                    do! Async.Sleep 2000
                                    formMessageVar.Set ""
                                } |> Async.Start
                            )
                        )
                    ] [
                        text "Delete"
                    ]
                ]
            ]
        ]

    let shiftTypeSelectorButton shiftType labelText =
        Doc.BindView (fun selected ->
            let buttonClass =
                if selected = shiftType then
                    "px-3 py-2 rounded-lg border text-sm font-medium transition bg-blue-600 text-white border-blue-600"
                else
                    "px-3 py-2 rounded-lg border text-sm font-medium transition bg-white text-gray-700 border-gray-300 hover:bg-gray-50"

            button [
                attr.``type`` "button"
                attr.``class`` buttonClass
                on.click (fun _ _ -> selectedShiftTypeVar.Set shiftType)
            ] [
                text labelText
            ]
        ) selectedShiftTypeVar.View

    let filterSelectorButton filter labelText =
        Doc.BindView (fun selected ->
            let buttonClass =
                if selected = filter then
                    "px-3 py-2 rounded-lg border text-sm font-medium transition bg-gray-800 text-white border-gray-800"
                else
                    "px-3 py-2 rounded-lg border text-sm font-medium transition bg-white text-gray-700 border-gray-300 hover:bg-gray-50"

            button [
                attr.``type`` "button"
                attr.``class`` buttonClass
                on.click (fun _ _ -> selectedFilterVar.Set filter)
            ] [
                text labelText
            ]
        ) selectedFilterVar.View
        

    let statsCards () =
        Doc.BindView (fun (shifts: ShiftEntry list) ->
            let totalCount = List.length shifts
            let dayCount = shifts |> List.filter (fun x -> x.ShiftType = Day) |> List.length
            let nightCount = shifts |> List.filter (fun x -> x.ShiftType = Night) |> List.length
            let restLeaveCount = shifts |> List.filter (fun x -> (x.ShiftType = Rest) || (x.ShiftType = Leave)) |> List.length

            section [attr.``class`` "grid md:grid-cols-3 gap-4"] [

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
            Doc.BindView (fun currentFilter ->
                let filteredShifts =
                    shifts
                    |> List.sortBy (fun x -> x.Date)
                    |> filterShifts currentFilter

                if List.isEmpty filteredShifts then
                    div [attr.``class`` "bg-white rounded-xl border border-gray-200 p-4 text-gray-500"] [
                        text "No shift entries found for this filter."
                    ]
                else
                    div [] (
                        filteredShifts
                        |> List.map renderShiftEntry
                    )
            ) selectedFilterVar.View
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

    let projectDetailsSection =
        details [attr.``class`` "bg-white rounded-2xl shadow-sm border border-gray-200 p-6"] [
            summary [attr.``class`` "text-xl font-semibold text-gray-800 cursor-pointer"] [
                text "About"
            ]

            div [attr.``class`` "mt-4 space-y-4"] [
                div [] [
                    h3 [attr.``class`` "text-lg font-semibold text-gray-800 mb-2"] [
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
                            Doc.InputType.Date [
                                attr.``class`` "w-full px-4 py-2 rounded-lg border border-gray-300"
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
                            Doc.BindView (fun editing ->
                                match editing with
                                | Some _ -> 
                                    button [
                                        attr.``class`` "ml-3 px-5 py-2 rounded-lg bg-gray-300 text-gray-800 font-medium"
                                        on.click (fun _ _ ->
                                            editingShiftVar.Set None
                                            dateInputVar.Set ""
                                            noteInputVar.Set ""
                                            selectedShiftTypeVar.Set Day
                                        )
                                    ] [
                                        text "Cancel"
                                    ]
                                | None -> Doc.Empty
                            ) editingShiftVar.View
                        ]
                    ]

                    formMessage ()
                ]
            ]

            section [attr.id "sample-shifts"] [
                div [attr.``class`` "flex flex-col gap-4 mb-4"] [
                    h2 [attr.``class`` "text-2xl font-semibold text-gray-800"] [
                        text "Shift entries"
                    ]
                    div [] [
                        p [attr.``class`` "text-sm font-medium text-gray-700 mb-2"] [
                            text "Filter"
                        ]

                        div [attr.``class`` "flex flex-wrap gap-2"] [
                            filterSelectorButton All "All"
                            filterSelectorButton OnlyDay "Day"
                            filterSelectorButton OnlyNight "Night"
                            filterSelectorButton OnlyRest "Rest"
                            filterSelectorButton OnlyLeave "Leave"
                        ]
                    ]
                ]

                shiftList ()
            ]

            projectDetailsSection

        ]

    [<SPAEntryPoint>]
    let Main () =
        IndexTemplate()
            .Content(pageContent)
            .Bind()