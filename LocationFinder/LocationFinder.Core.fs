namespace LocationFinder

module Core =

    open System

    type Availability = Available | NotAvailable

    type Point = { lat: double; long: double; name: string; }  

    type Coordinates = { x: double; y: double; z: double }          
         
    type TimeFrame = { day: DayOfWeek; start: TimeSpan; finish: TimeSpan }

    type LocationSlot = { point: Point; time: TimeFrame; }

    type Result = { location: LocationSlot; distance: float; fromLocation: LocationSlot; availability: Availability }

    [<Literal>]
    let earthRad = 6378.1370  //assuming miles here

    let inline toRadians deg = (Math.PI / 180.0) * deg

    let toCoordinates point = 
        {  x = cos (point.lat |> toRadians) * cos (point.long |> toRadians); 
           y = cos (point.lat |> toRadians) * sin (point.long |> toRadians);
           z = sin (point.lat |> toRadians); }

    let getDistance fromPoint toPoint = 
        let fromCoord = fromPoint |> toCoordinates
        let toCoord = toPoint |> toCoordinates
        Math.Round((earthRad * acos (toCoord.x * fromCoord.x + toCoord.y * fromCoord.y + toCoord.z * fromCoord.z)), 1)

    let getAvailability = function
                        | (distance, _, _) when distance > 20.0 -> NotAvailable
                        | (_, time1, time2) when time1.day <> time2.day -> NotAvailable
                        | (_, time1, time2) when time1.start < time2.finish && time2.start < time1.finish -> Available
                        | _ -> NotAvailable
     
    let getResult dropOff customer =
        let distance = getDistance customer.point dropOff.point
        let availability = getAvailability (distance, customer.time, dropOff.time) 
        { location = dropOff; 
          distance = distance; 
          fromLocation = customer;
          availability = availability; }

    let defaultSort result = result.availability, result.distance

    let getBestResult customerLocations dropOffLocation =
        customerLocations 
        |> Seq.map (getResult dropOffLocation)
        |> Seq.sortBy defaultSort
        |> Seq.head

    let findLocations dropOffLocations customerLocations =
        dropOffLocations
        |> Seq.map (getBestResult customerLocations)
        |> Seq.sortBy defaultSort

