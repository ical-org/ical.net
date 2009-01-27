using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace DDay.iCal.Validator
{
    public interface IValidationSerializer
    {
        /// <summary>
        /// Gets the default extension for files that
        /// use this serializer (without the leading dot '.').
        /// </summary>
        string DefaultExtension { get; }
        Encoding DefaultEncoding { get; }

        IValidationRuleset Ruleset { get; set; }
        ITestResult[] TestResults { get; set; }
        IValidationResult[] ValidationResults { get; set; }

        /// <summary>
        /// Serializes test and validation results.
        /// </summary>
        /// <param name="stream">The stream to serialize results to.</param>
        /// <param name="encoding">The encoding to use during serialization.</param>
        void Serialize(Stream stream, Encoding encoding);
    }
}
