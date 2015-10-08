module ``Location Finder Tests``

open NUnit.Framework
open FsUnit
open LocationFinder.Core
open System

let home = { lat = 44.948265; long = -93.153313; name = "Home"; }
let work = { lat = 44.952409; long = -92.994088; name = "Work"; }
let homeMorning = { point = home; time = { day = DayOfWeek.Monday; start = new TimeSpan(0, 0, 0); finish = new TimeSpan(8, 0, 0)} }
let workDay = { point = work; time = { day = DayOfWeek.Monday; start = new TimeSpan(8, 0, 0); finish = new TimeSpan(17, 0, 0); } }
let homeEvening = { point = home; time = { day = DayOfWeek.Monday; start = new TimeSpan(17, 0, 0); finish = new TimeSpan(24, 0, 0); } }

let customerData = [ homeMorning; workDay; homeEvening; ]

let sunRay =  { point = { lat = 44.952118; long = -93.010582; name ="Sun Ray Shopping Center"; }; time = { day = DayOfWeek.Monday; start = new TimeSpan(14,0,0); finish = new TimeSpan(15,0,0) } }
let chsField = { point = { lat = 44.950739; long = -93.083872; name ="CHS Field"; }; time = { day = DayOfWeek.Monday; start = new TimeSpan(10,0,0); finish = new TimeSpan(12,0,0) } }
let washingtonHigh = { point = { lat = 44.985683; long = -93.109708; name ="Washington High School"; }; time = { day = DayOfWeek.Monday; start = new TimeSpan(17,30,0); finish = new TimeSpan(19,00,0) } }
let stateFair = { point = { lat = 44.979895; long = -93.169483; name ="State Fairgrounds"; }; time = { day = DayOfWeek.Tuesday; start = new TimeSpan(10,0,0); finish = new TimeSpan(11,30,0) } }
let target = { point = { lat = 44.954618; long = -93.155481; name ="Midway Target"; }; time = { day = DayOfWeek.Monday; start = new TimeSpan(7,0,0); finish = new TimeSpan(10,0,0) } }
let mayoClinic = { point = { lat = 44.022943; long = -92.466627; name ="Mayo Clinic - Rochester"; }; time = { day = DayOfWeek.Monday; start = new TimeSpan(7,0,0); finish = new TimeSpan(10,0,0) } }

let dropoffs = [ sunRay; chsField; washingtonHigh; stateFair; target; ]   

   
module ``Core`` =    

    [<Test>]
    let ``All locations should be listed by distance ascending within time-available/not-available grouping`` () =
        let result = findLocations dropoffs customerData
        result 
        |> Seq.filter (fun r -> r.availability = Available) 
        |> Seq.map (fun r -> r.distance)
        |> should be ascending 


    [<Test>]
    let ``All locations should be listed only once with best distance & availablity`` () =
        let result = findLocations dropoffs customerData
        result |> should be unique
        result |> Seq.toList |> should haveLength 5

    [<Test>]
    let ``Closest time-available slot should be first`` () =
        let topResult = findLocations dropoffs customerData |> Seq.head
        topResult.location |> should equal target

    [<Test>]
    let ``Time-available slots beyond a reasonable drive should be treated as not available`` () =
        let bottomResult = findLocations [ stateFair; mayoClinic; ] customerData |> Seq.last
        bottomResult.location |> should equal mayoClinic