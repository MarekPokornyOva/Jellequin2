import consoleDll from "System.Console, Version = 4.1.0.0, Culture = neutral, PublicKeyToken = b03f5f7f11d50a3a";
//debugger;
var args = arguments;
var console = consoleDll.System.Console;
var wl = console.WriteLine;
wl("Jellequin application is alive!");

function parseArg(arg) {
	var fch = arg.substr(0, 1);
	var arg2 = (fch == '/') || (fch == '-') ? arg.substr(1, arg.length - 1) : arg;
	var pos = arg2.indexOf(" = ");
	var n; var v;
	//wl(pos);
	if (pos == -1) {
		n = arg2;
		v = "";
	}
	else {
		n = arg2.substr(0, pos);
		v = arg2.substr(pos + 1, arg2.length);
	}
	return { orig: arg2, name: n, value: v };
}

var argsLen = args.length;
if (argsLen == 0) {
	wl("No arguments");
	//parseArg("NoArg");
}
for (var a = 0; a < argsLen; a++) {
	var arg = parseArg(args[a]);
	wl("arg "+ (a + 1) + ": " + arg.name + '="' + arg.value + '"');
}
console.Write("Press any key to continue...");
console.ReadKey();
console.WriteLine();
console.WriteLine();
