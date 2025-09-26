using System.Runtime.CompilerServices;

// Allow certain assemblies to see the classes or methods decorated with the "internal" keyword
[assembly: InternalsVisibleTo("SMTPRouter.Listener")]
[assembly: InternalsVisibleTo("SMTPRouter.Router")]
[assembly: InternalsVisibleTo("SMTPRouter.UnitTesting")]
