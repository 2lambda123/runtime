// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

.assembly extern System.Console { }
.assembly extern xunit.core {}
/* RDawson - 11/2/200

   Check that interfaces can have static fields
*/

.assembly Bob{}
.assembly extern mscorlib {}

.class interface Foo{
	.field public static int32 Bar
}

.class public Bar implements Foo{
	.method public specialname void .ctor(){
		ldarg.0
		call	instance void [mscorlib]System.Object::.ctor()
		ret
	}
}

.method public static int32 main(){
	.custom instance void [xunit.core]Xunit.FactAttribute::.ctor() = (
	    01 00 00 00
	)
	.entrypoint
	.maxstack 8
	newobj instance	void Bar::.ctor()
	pop
	ldstr	"PASS - 2.8.31 - Was allowed to define an interface with a static field"
	call	void [System.Console]System.Console::WriteLine(class [mscorlib]System.String)
	ldc.i4	100
	ret
}
