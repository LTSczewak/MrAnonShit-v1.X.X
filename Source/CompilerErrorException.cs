using System;

namespace MrAnonCrypter
{
  internal class CompilerErrorException : Exception
  {
    public CompilerErrorException()
    {
    }

    public CompilerErrorException(string message)
      : base(message)
    {
    }

    public CompilerErrorException(string message, Exception inner)
      : base(message, inner)
    {
    }
  }
}