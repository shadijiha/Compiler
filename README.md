# High-level compiler

Built a complex high-level language compiler from scatch. The compiler supports lots of features such as: if/else, for loops, classes, functions, pointers and more!
This compiler combines my 2 favorite language syntaxe's: C++ and Java syntax into 1.

## Main goal
The main goal was to have a nice scripting language what goes well with my <a href="https://github.com/shadijiha/shado-opengl-api">Game Engine</a>

Here's a sample code that the compiler is able to execute:

```
#include "__PATH_TO_CORE__/core.sscript"    // All standard lib definitions are here

/* Classses support */
class TestClass {

    public void sayHello()  {
        string name = TestClass.class();
        println("Hello from -->", name);
    }

    public static void staticFunction(int param)    {
        println("Hello from static --> ", param);
    }
}

void change(int* i) {
    *i = 200;
}

int main(int argc, string[] argv)	{
    print("Enter value for a > ");
    string val = input();

    /* Pointers */   
    int a = val;
    change(&a);
    println(a);

    /* Arrays (Check all arrays operations in core.sscript) */
    double[] array = {1.1, 3.145, 530.0};
    double index2 = array.at(2);
    println(index2);
    
    array.add(pi());
    println(array)
    println("---------------------------------");

    /* Strings (Check all string operations in core.sscript) */
    string myString = "Hello world!";
    string upper = myString.toUpperCase();
    println(upper);

    /* Objects */
    TestClass test = new TestClass();
    test.sayHello();
    TestClass.staticFunction(10);

    /* IO */
    FileWriter writer = new FileWriter("__PATH__/text.txt");
    writer.open();
    writer.writeln("Text!!!!");
    writer.close();     /* This class also implements AutoClosable, meaning it will close when program 
                           Terminates even if you don't call close() */

    FileReader reader = new FileReader("__PATH__/text.txt");
    reader.open();
    string text = reader.readToEnd();
    println(text);
    reader.close();
}
```
         
   
