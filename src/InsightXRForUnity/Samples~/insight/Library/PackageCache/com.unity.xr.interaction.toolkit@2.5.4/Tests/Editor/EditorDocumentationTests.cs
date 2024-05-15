using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace UnityEditor.XR.Interaction.Toolkit.Editor.Tests
{
    [TestFixture]
    class EditorDocumentationTests
    {
        /// <summary>
        /// <see cref="PackageManager.PackageInfo"/> for com.unity.xr.interaction.toolkit.
        /// </summary>
        PackageManager.PackageInfo m_PackageInfo;

        [OneTimeSetUp]
        public void SetUp()
        {
            var assembly = Assembly.Load("Unity.XR.Interaction.Toolkit");
            Assert.That(assembly, Is.Not.Null);

            m_PackageInfo = PackageManager.PackageInfo.FindForAssembly(assembly);
            Assert.That(m_PackageInfo, Is.Not.Null);
            Assert.That(m_PackageInfo.version, Is.Not.Null);
            Assert.That(m_PackageInfo.version, Is.Not.Empty);
        }

        [Test]
        public void HelpURLVersionMatchesPackageVersion()
        {
            var majorMinorVersionString = GetMajorMinor(m_PackageInfo.version);
            Assert.AreEqual(majorMinorVersionString, XRHelpURLConstants.currentDocsVersion);
        }

        [Test]
        public void InstallationVersionMatchesPackageVersion()
        {
            // Verify that the 2021.3 installation instructions to install the package by name
            // has the correct version of this package.

            const string docRoot = "Packages/com.unity.xr.interaction.toolkit/Documentation~";
            var documentationDirectory = new DirectoryInfo(docRoot);
            Assert.That(documentationDirectory, Is.Not.Null);
            Assert.That(documentationDirectory.Exists, Is.True);

            var installationFile = new FileInfo(Path.Combine(documentationDirectory.FullName, "installation.md"));
            Assert.That(installationFile, Is.Not.Null);
            Assert.That(installationFile.Exists, Is.True);

            var lines = File.ReadAllLines(installationFile.FullName);
            Assert.That(lines, Is.Not.Null);
            Assert.That(lines, Is.Not.Empty);

            // Find the lines in the file with the Version (optional) row in the table
            var installationVersionLines = lines.Where(line => line.StartsWith("|**Version (optional)**|")).ToArray();
            Assert.That(installationVersionLines, Is.Not.Empty, "Could not find version row of table. Has installation.md been updated to remove instructions for installing by name?");

            // Parse version
            foreach (var line in installationVersionLines)
            {
                var version = ExtractCode(line, "**[", "]**");
                Assert.That(version, Is.Not.Empty, $"Could not parse the version field from the table line: {line}");
                Assert.That(version, Is.EqualTo("!include[](includes/version.md)"), "Version string in installation.md should match current version of package.");
            }

            var installationKharmaLinkVersionLines = lines.Where(line => line.Contains($"(com.unity3d.kharma:upmpackage/com.unity.xr.interaction.toolkit@")).ToArray();
            Assert.That(installationKharmaLinkVersionLines, Is.Not.Empty, "Could not find version kharma link. Has installation.md been updated to remove kharma links?");
            // Parse version
            foreach (var line in installationKharmaLinkVersionLines)
            {
                var version = ExtractCode(line, "@", ")");
                Assert.That(version, Is.Not.Empty, $"Could not parse the version field from the kharma link: {line}");
                Assert.That(version, Is.EqualTo(m_PackageInfo.version), "Version strings in installation.md kharma links should match current version of package.");
            }

            // Setup includes folder for parsing
            var includesDirectory = new DirectoryInfo(Path.Combine(docRoot, "includes"));
            Assert.That(includesDirectory, Is.Not.Null);
            Assert.That(includesDirectory.Exists, Is.True);

            var versionFile = new FileInfo(Path.Combine(includesDirectory.FullName, "version.md"));
            Assert.That(versionFile, Is.Not.Null);
            Assert.That(versionFile.Exists, Is.True);

            // Read all lines and make sure it's not empty or null and there is only 1 line in the file to prevent malformed documentation.
            var versionLines = File.ReadAllLines(versionFile.FullName);
            Assert.That(versionLines, Is.Not.Null);
            Assert.That(versionLines, Is.Not.Empty);
            Assert.That(versionLines.Length, Is.EqualTo(1));

            // Grab the first line of the file 
            var versionLine = versionLines.First().Trim();
            Assert.That(versionLine, Is.Not.Empty, "Could not find version line. Has version.md been erased or removed?");

            // Parse version
            Assert.That(versionLine, Is.Not.Empty, $"Could not parse the version from the file line: {versionLine}");
            Assert.That(versionLine, Is.EqualTo(m_PackageInfo.version), $"Version string in version.md should match current version of package: {versionLine} vs {m_PackageInfo.version}");
        }

        [Test]
        public void PackageDependencyLinksToCurrentVersionDependency()
        {
            // Verify that within the documentation, any package references to a specific version
            // matches the dependency versions of this package.

            var documentationDirectory = new DirectoryInfo("Packages/com.unity.xr.interaction.toolkit/Documentation~");
            Assert.That(documentationDirectory, Is.Not.Null);
            Assert.That(documentationDirectory.Exists, Is.True);

            // Create two lists of all package dependencies in the forms:
            // name
            // name@major.minor
            var nameNoVersions = new List<string>(m_PackageInfo.dependencies.Length);
            var nameAndVersions = new List<string>(m_PackageInfo.dependencies.Length);
            nameNoVersions.AddRange(m_PackageInfo.dependencies.Select(dependency => dependency.name));
            nameAndVersions.AddRange(m_PackageInfo.dependencies.Select(dependency => $"{dependency.name}@{GetMajorMinor(dependency)}"));

            // Package name must contain only lowercase letters, digits, hyphens(-), underscores (_), and periods (.)
            // See https://docs.unity3d.com/Manual/cus-naming.html
            // Regex in pattern name@major.minor
            var regex = new Regex(@"com\.unity\.[a-z0-9-_.]+@\d+\.\d+");
            foreach (var nameAndVersion in nameAndVersions)
            {
                Assert.That(regex.IsMatch(nameAndVersion), Is.True);
            }

            foreach (var fileInfo in documentationDirectory.EnumerateFiles("*.md"))
            {
                var lines = File.ReadAllLines(fileInfo.FullName);
                var lineNumber = 0;
                foreach (var line in lines)
                {
                    ++lineNumber;
                    foreach (Match match in regex.Matches(line))
                    {
                        var reference = match.ToString();
                        // Skip requiring external references to specific versions of the package that aren't a package dependency.
                        // This is because we do not have a dependency on com.unity.xr.openxr
                        // but sometimes want to link to a specific version of that package.
                        var referencePackageName = reference.Split('@')[0];
                        if (nameNoVersions.Contains(referencePackageName))
                        {
                            Assert.That(nameAndVersions, Has.Member(reference), $"{reference} in {fileInfo.Name}:{lineNumber} does not match dependency version.");
                        }
                        else
                        {
                            Debug.LogWarning($"{reference} in {fileInfo.Name}:{lineNumber} has a specific version but {referencePackageName} is not a package dependency.");
                        }
                    }
                }
            }
        }

        static string ExtractCode(string value, string start, string end)
        {
            // Extract the placeholder text from the string: **[!include[](includes/version.md)]**
            var firstIndex = value.IndexOf(start);
            var lastIndex = value.LastIndexOf(end);
            if (firstIndex == -1 || lastIndex == -1 || firstIndex == lastIndex)
                return string.Empty;

            return value.Substring(firstIndex + start.Length, lastIndex - firstIndex - end.Length);
        }

        static string GetMajorMinor(DependencyInfo dependency) => GetMajorMinor(dependency.version);

        static string GetMajorMinor(string version)
        {
            // Return major.minor from the string.
            // For example, "1.2.3" would return "1.2"
            Assert.That(version, Is.Not.Null);
            Assert.That(version, Is.Not.Empty);
            var secondDotIndex = version.IndexOf('.', version.IndexOf('.') + 1);
            Assert.That(secondDotIndex, Is.GreaterThan(0));
            return version.Substring(0, secondDotIndex);
        }
    }
}