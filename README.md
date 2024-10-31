# Wagago

This repo is for my implementations of the "jlox" interpreter from [Crafting Interpreters](https://craftinginterpreters.com/).

"wagago" is romaji for "我が語" which can be loosely translated into "my language"

## C# interpreter

This interpreter is almost identical to the "jlox" interpreter outlined for Java in the Crafting 
Interpreters book.

Some changes include:
* Support for modules via files

## Building
1. Build the `tool` solution
2. Generate the AST using the `tool` binary in either the `Debug` or `Release` directory and 
   the `wagago` project directory as a destination. There's a run configuration in the `tool` 
   project directory.
3. Build the `wagago` solution.
4. Run the `wagago` binary from the command line or against a file.

## Example program
```
fun makeCounter() {
  var i = 0;
  fun count() {
    i = i + 1;
    print i;
  }

  return count;
}

var counter = makeCounter();
counter(); // "1".
counter(); // "2".

var a = "global";
{
  fun showA() {
    print a;
  }

  showA();
  var a = "block";
  showA();
}

class Brot {
    serveOn() {
        print "zum Fruhstuck";
    }
}

class Vollkorn < Brot {
    serveOn() {
        super.serveOn();
        print "Whole wheat";
    }
}

Vollkorn().serveOn();
```
