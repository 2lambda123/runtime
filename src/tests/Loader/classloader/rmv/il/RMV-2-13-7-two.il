// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

.assembly extern System.Console { }
.assembly extern xunit.core {}
/*RDawson

  Check that methods can be virtual and not abstract
*/

.assembly Bob{}
.assembly extern mscorlib {}

.class public Foo{
	.method public virtual void foo(){
		ret
	}
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
	newobj instance	void Foo::.ctor()
	pop
	ldstr	"PASS - 2.13.7 - Was allowed to declare a method as virtual and not abstract"
	call	void [System.Console]System.Console::WriteLine(class [mscorlib]System.String)
	ldc.i4	100
	ret
}
