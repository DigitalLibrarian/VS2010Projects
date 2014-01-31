"DebugTerminal.pdb", "DebugTerminal.dll", and "DebugTerminal.xml" were built using Visual Studio 2010 and XNA 4.0
"Documentation.chm" was built using Sandcastle Help File Builder GUI 1.9.1.0 from http://shfb.codeplex.com/
"Antlr3.Runtime.dll" came from http://www.antlr.org/

Adding to your XNA project:
    Open a pre-existing or new XNA project in Visual Studio. 
	In Solution Explorer, right click on 'References' (from the root list not the one in the Content directory)
	Choose 'Add Reference' and click on the 'Browse' tab. 
	Browse to where "DebugTerminal.dll" was downloaded, select it, and hit Ok.
	Note that "Antlr3.Runtime.dll" must be in the same directory as "DebugTerminal.dll" as "DebugTerminal.dll" references it.
		To ensure this you can add a reference to "Antlr3.Runtime.dll" the same way you did "DebugTerminal.dll", however this
		shouldn't be necessary if they are in the same directory

For more information please visit 
	http://www.protohacks.net/xna_debug_terminal/