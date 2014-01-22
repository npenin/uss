#region Using directives

using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.ConstrainedExecution;

#endregion

//
// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
//
[assembly: AssemblyTitle(@"")]
[assembly: AssemblyDescription(@"")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany(@"Evaluant")]
[assembly: AssemblyProduct(@"Evaluant.Uss")]
[assembly: AssemblyCopyright("")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: System.Resources.NeutralResourcesLanguage("en")]

//
// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Revision and Build Numbers 
// by using the '*' as shown below:

[assembly: AssemblyVersion(@"1.0.0.0")]
[assembly: ComVisible(false)]
[assembly: CLSCompliant(true)]
[assembly: ReliabilityContract(Consistency.MayCorruptProcess, Cer.None)]

//
// Make the Dsl project internally visible to the DslPackage assembly
//
[assembly: InternalsVisibleTo(@"Evaluant.Uss.SqlMapper.Mapping.DslPackage, PublicKey=00240000048000009400000006020000002400005253413100040000010001000B12E7850E842FF560DA209AA13D4E372CE9A8DE90805F4174B00FE3A005440F28C83B496961A69319EB86587667A0FA3254FE5E3E07B4E54F5DCE30C395A237053D62009BD12026B41E8D179CBF942FACAB770DE19E91A31ECE6D53EC7E567A4F2386E99887A598A37E03A33B1A0A6908BCDA190CD7897430C96809FF92A9E6")]