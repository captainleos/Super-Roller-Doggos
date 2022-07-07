// -----------------------------------------------------------------------
// <copyright file="IFileFormat.cs" company="">
// Triangle.NET code by Christian Woltering, http://triangle.codeplex.com/
// </copyright>
// -----------------------------------------------------------------------

namespace TerraUnity.TriangleNet.IO
{
    public interface IFileFormat
    {
        bool IsSupported(string file);
    }
}
