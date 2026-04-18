namespace DUE_FSharp_SPASandbox_2026

open System
open WebSharper
open WebSharper.UI
open WebSharper.UI.Html
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

                span [attr.``class`` ("inline-block px-3 py-1 rounded-full text-sm font-medium " + shiftTypeBadgeClass entry.ShiftType)] [
                    text (shiftTypeToText entry.ShiftType)
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
                        text "This is the first project version with a custom layout and static sample shift entries."
                    ]
                ]
            ]

            section [attr.``class`` "grid md:grid-cols-3 gap-4"] [
                div [attr.``class`` "bg-blue-50 border border-blue-200 rounded-xl p-4"] [
                    h2 [attr.``class`` "text-lg font-semibold text-blue-800 mb-2"] [
                        text "Current stage"
                    ]
                    p [attr.``class`` "text-blue-700"] [
                        text "Initial project skeleton"
                    ]
                ]

                div [attr.``class`` "bg-white border border-gray-200 rounded-xl p-4"] [
                    h2 [attr.``class`` "text-lg font-semibold text-gray-800 mb-2"] [
                        text "Entries"
                    ]
                    p [attr.``class`` "text-gray-600"] [
                        text (string sampleShifts.Length + " sample shifts")
                    ]
                ]

                div [attr.``class`` "bg-white border border-gray-200 rounded-xl p-4"] [
                    h2 [attr.``class`` "text-lg font-semibold text-gray-800 mb-2"] [
                        text "Status"
                    ]
                    p [attr.``class`` "text-gray-600"] [
                        text "Static preview version"
                    ]
                ]
            ]

            section [attr.id "sample-shifts"] [
                div [attr.``class`` "flex items-center justify-between mb-4"] [
                    h2 [attr.``class`` "text-2xl font-semibold text-gray-800"] [
                        text "Sample shift entries"
                    ]
                ]

                div [] (
                    sampleShifts
                    |> List.map renderShiftEntry
                )
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
                        text "Later versions will include adding shifts, deleting entries, sorting, summaries, and workload analysis."
                    ]
                ]
            ]
        ]

    [<SPAEntryPoint>]
    let Main () =
        IndexTemplate()
            .Content(pageContent)
            .Bind()