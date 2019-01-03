﻿//  ______                _   _                 
// |  ____|              | | (_)                
// | |__ _   _ _ __   ___| |_ _  ___  _ __  ___ 
// |  __| | | | '_ \ / __| __| |/ _ \| '_ \/ __|
// | |  | |_| | | | | (__| |_| | (_) | | | \__ \
// |_|   \__,_|_| |_|\___|\__|_|\___/|_| |_|___/

open System
module Functions =
    
    // Use 'let' to define a function that accepts an integer argument and returns an integer.    
    // The "let" keyword also defines a named function.
    let square x = x * x          // Note that no parens are used.
    square 3                      // Now run the function. Again, no parens.

    // Follow the arrows
    // int -> int -> int     
    let add x y = x + y           // don't use add (x,y)! It means something
                                  // completely different.
    add 2 3                       // Now run the function.

    let addFunc = fun a b -> a + b


    let z = add 1 2

    let add42 = add 42

    let addOne = add 1 // partial appliaction
    let addTwo = add 2
    
    addOne 5


    // f(x) = x + 2    
    let add2 number = number + 2

    // use the new function
    add42 2
    add42 3




    // Same function as above but with type annoations
    // Why is it add2' ("add two prime")?
    // Functions are values and are immutable (so no function overloading.
    let add2' (x : int) int : int = x + 2

    // Another function
    // Mouse over function name to see that it
    // b:float -> e:float -> float
    // Compiler infers that inputs are floats since they're passed to a method that accepts floats
    let power b e = Math.Pow(b, e)

    // Type annotated version of power
    let power' (b : float) (e : float)  float = Math.Pow(b, e)

    let power'' = Math.Pow


     // Because F# is statically typed, calling the add method 
    // you just created with a floating-point value will result in a compiler error
    let Add x y = x + y

    let resultFloat = Add 1.2 2.6
    let resultint = Add 1 2
        
    //	But the + operator also works on floats too !?!?!?
    //  The reason is due to type inference. 
    //  Because the + operator works for many different types, such as byte, int, and decimal
    //	the compiler simply defaults to int if there is no additional information
    let inline op x y = x + y  // The presence of inline affects type inference. 
                                 // This is because inline functions can have 
                                 // statically resolved type parameters

    let r1 = op 1 2
    let r2 = op 1.4 2.5
    let r3 = op "Hello " "World"

    
    
    let oneToFive = [1..5]

    // to define a multiline function, just use indents. No semicolons needed.
    let evens list =
       let isEven x = x%2 = 0     // Define "isEven" as a sub function
       List.filter isEven list    // List.filter is a library function
                                  // with two parameters: a boolean function
                                  // and a list to work on

    evens oneToFive               // Now run the function


    // A quick intro HOF
    // mapAList partially applies the map function
    // map function accepts a function and a list
    let mapAList =
        //  List.map ('a -> 'b) -> 'a list -> 'b list
        List.map (fun i -> i * i)

    mapAList oneToFive

    // printAList partial applies the
    // iter function from the List module
    // iter accepts a function and a list
    let printAList =
        // List.iter ('a -> unit) -> 'a list -> unit 
        List.iter (fun i -> printfn "%i" i) 

    printAList oneToFive

    // ----------------------------------------------------------------------------

    // Everything is a function (+)
    // Print the truth table for the given function
    let printTruthTable f =
        printfn "       |true   | false |"
        printfn "       +-------+-------+"
        printfn " true  | %5b | %5b |" (f true true)  (f true false)
        printfn " true  | %5b | %5b |" (f false true) (f false false)
        printfn "       +-------+-------+"
        printfn ""
        ()
    
    printTruthTable (&&)

    printTruthTable (||)

    // Compute the factorial of an integer. Use 'let rec' to define a recursive function    
    // recursive functions are composable and don't mutate state
    let rec factorial n =
        if n = 0 then 1
        else n * factorial (n - 1)

    printfn "%d" <| factorial 5
   

    // Define mutually recursive functions
    let rec isOdd  n = 
        if   n = 0 then false 
        elif n = 1 then true
        else isEven (n - 1)
    and isEven n = 
        if   n = 0 then true 
        elif n = 1 then false
        else isOdd (n - 1)

    let is5Odd = isOdd 5
    

    (*operator overload*)
    let rec (!) x =
        if x <= 1 then 1
        else x * !(x - 1)


module ``Kitten Functions`` =

    type Kitten = { Name: string; Age: int }

    // List comprehensions: 
    // let's just create a list of kittens 
    let kittens = [
        { Name = "Alfred"; Age = 5 };
        { Name = "Biggie"; Age = 12 };
        { Name = "Cat"; Age = 2 }; ]

    // this is not really needed here,
    // but introduced for clarity
    type Filter = Kitten -> bool

    // List, Seq, ... modules provide
    // functionality similar to LINQ
    let giveMeKittens (kittens: Kitten list) (filter: Filter) =
        List.filter filter kittens

    // List.

    // You can declare a function like this;
    // note we don't need parenthesis
    let youngOne kitten = kitten.Age < 3

    // the signature happens to match,
    // so no fuss no muss you can just use it:
    let youngUns = giveMeKittens kittens youngOne

    // more interesting: a function with 2 arguments
    // note that we still have no parenthesis
    let youngerThan age kitten = kitten.Age < age

    // Why would that be a good idea?
    // First you can create a new function on the fly,
    // by leaving some arguments "free", and
    // "locking" some arguments (here we use a closure on 10):
    let youngerThan10 = youngerThan 10

    // this is handy in situations like this:
    let stillYoung = giveMeKittens kittens (youngerThan 6)

    // using pipe-forward, you can now replace f x y by y |> f x 
    let oldSchool = youngerThan 6 { Name = "Albert"; Age = 11 }
    let coolBeans = { Name = "Friday"; Age = 6 } |> youngerThan 5

    // why is this awesomesauce?
    // essentially you can freely compose functions,
    // you get fluent interfaces everywhere, for free

    let lowerCase (text: string) = text.ToLowerInvariant()
    let split (text: string) = text.Split(' ')

    let cleanup text = 
        text 
        |> lowerCase // lowerCase text
        |> split // split (lowerCase text)
        |> Array.sort // ...


// =====================================
// ==========   Composition  ===========
// =====================================
module ``Composition - Pipe Operaror`` =

    // Forward Pipe Operator
    // The Forward pipe operator is simply defined as:

    
    let multiBy2 x         = x * 2
    let toStr (x : int)  = x.ToString()
    let rev   (x : string) = new String(Array.rev (x.ToCharArray()))

    // 512 -> 1024 -> "1024" -> "4201"
    let result = rev (toStr (multiBy2 512))

    // let (|>) x f = f x
    //'a -> ('a -> 'b) -> 'b

    let result' = 512 |> multiBy2 |> toStr |> rev
    
    [1..1000]
    |> List.filter (fun t -> t % 2 = 0)
    |> List.map (fun t -> t * t)
    |> List.filter (fun t -> t < 1000)


    let myList = [1..20]
    let printAList = List.iter (fun i -> printfn "%d" i)
    let mapAList = List.map (fun i -> i * 2)

    let printAList' = List.iter (printfn "%d")
    let mapAList' = List.map multiBy2

    // Forward pipelining
    // Forwards the result of a function to the last argument of another function.
    myList |> printAList
    myList |> mapAList |> printAList


    
    
    // let (>>) g f a = f(g(a))
    // (('a -> 'b) -> ('b -> 'c) -> 'a -> 'c) 

    // building blocks
    let add2 x = x + 2
    let mult3 x = x * 3
    let square x = x * x

    // test
    [1..10] |> List.map add2 |> printfn "%A"
    [1..10] |> List.map mult3 |> printfn "%A"
    [1..10] |> List.map square |> printfn "%A" 
    
    let add7 number = number + 7
    let add5 number = number + 5

    let add12 = add5 >> add7
    let value = add12 7



    // Functional Composition
    let printMappedList = mapAList >> printAList
    printMappedList myList


    // new composed functions
    let add2ThenMult3 = add2 >> mult3
    let mult3ThenSquare = mult3 >> square 

    let add2ThenMult3' x = mult3 (add2 x)
    let mult3ThenSquare' x = square (mult3 x) 

    add2ThenMult3 5
    mult3ThenSquare 5

    [1..10] |> List.map add2ThenMult3 |> printfn "%A"
    [1..10] |> List.map mult3ThenSquare |> printfn "%A"



    // helper functions;
    let logMsg msg x = printf "%s%i" msg x; x     //without linefeed 
    let logMsgN msg x = printfn "%s%i" msg x; x   //with linefeed

    // new composed function with new improved logging!
    // Very simple to inject extra functionality respecting the type signature
    let mult3ThenSquareLogged = 
       logMsg "before=" 
       >> mult3 
       >> logMsg " after mult3=" 
       >> square
       >> logMsgN " result=" 

    mult3ThenSquareLogged 5
    [1..10] |> List.map mult3ThenSquareLogged //apply to a whole list



    let listOfFunctions = [
       mult3; 
       square;
       add2;
       logMsgN "result=";
       ]

    // compose all functions in the list into a single one
    let allFunctions = List.reduce (>>) listOfFunctions 

    allFunctions 5


module CurryingAndPartialApplication =

    // non curried function
    let saySomethingShort x y =
            x + " " + y

    // curried function
    // If you compare the functions they have the same signature
    let saySomethingShort' x =
        (fun y -> sprintf "%s %s" x y) // lambda evaluates and result is returned

    // Partially applied function
    // the result of passing "hello" to saySomethingShort is a function that accepts a string
    let sayHelloTo = saySomethingShort "Hello"
    printfn "%s" (sayHelloTo "World") // Hello World

    // Practical Partial Application
    
    let saySomethingShort'' = sprintf "%s %s"


    let formatUrl = sprintf "%s/%s" //function that accepts two strings...

    // Array.Filter accepts a function to use for filtering and the array to filter
    // If we don't pass the array then we get a function that accepts an array as its last argument

    let filterByName name = Array.filter (fun i -> i = name)
    let filterByJane = filterByName "Jane"

    let result = filterByName "Jane" [|"Dick";"Jane"|]

    let result' = [|"Dick";"Jane"|] |> filterByName "Jane"

    // more piping
    let isFactor f x = x % f = 0 
    let isEven = isFactor 2

    let sumOfEvens args =
        args
        |> Array.filter isEven
        |> Array.sum

    let sum = sumOfEvens [|1..100|] // Initialize an array of 1 to 100