using System;

namespace Unity.XR.CoreUtils.Editor
{
    /// <summary>
    /// Defines a validation rule used for assessing package setup correctness.
    /// </summary>
    /// <remarks>
    /// Use <see cref="CheckPredicate"/> to define the validation logic. Use <see cref="FixIt"/> to
    /// provide logic correct the validation problem. The other properties of `BuildValidationRule`
    /// define when the rule function is executed and how the rule is displayed in the Editor.
    ///
    /// See [Project Validation](xref:xr-core-utils-project-validation) for more information.
    /// </remarks>
    public class BuildValidationRule
    {
        /// <summary>
        /// Lambda function that shows the rule in the project validation window UI if a condition is met.
        /// By default all rules are shown.
        /// </summary>
        public Func<bool> IsRuleEnabled { get; set; } = () => true;

        /// <summary>
        /// Name of the rule that will be shown to the developer in the build validation drawer.
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Message describing the rule that will be shown to the developer when this rule fails.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Lambda function that returns <see langword="true"/> if validation passes.
        /// Otherwise, returns <see langword="false"/>.
        /// </summary>
        /// <remarks>
        /// By default the validation fails, equivalent to a function that returns <see langword="false"/>.
        /// </remarks>
        public Func<bool> CheckPredicate { get; set; }

        /// <summary>
        /// Lambda function that fixes the issue, if possible.
        /// </summary>
        public Action FixIt { get; set; }

        /// <summary>
        /// Text describing how the issue is fixed, shown in a tooltip.
        /// </summary>
        public string FixItMessage { get; set; }

        /// <summary>
        /// Returns <see langword="true"/> if the <see cref="FixIt"/> Lambda function performs a function that is automatic and does not require user input.
        /// If your `FixIt` function requires user input, set `FixItAutomatic` to <see langword="false"/> to prevent the `FixIt` method from
        /// being executed when the user clicks the **Fix All** button in the **Project Validation** category of **Project Settings**.
        /// </summary>
        public bool FixItAutomatic { get; set; } = true;

        /// <summary>
        /// If <see langword="true"/>, failing the rule is treated as an error and stops the build.
        /// If <see langword="false"/>, failing the rule is treated as a warning and it doesn't stop the build. The developer has the
        /// option to correct the problem, but is not required to.
        /// </summary>
        public bool Error { get; set; }

        /// <summary>
        /// Optional text to display in a help icon with the issue in the validator.
        /// </summary>
        public string HelpText { get; set; }

        /// <summary>
        /// Optional link that will be opened if the help icon is clicked.
        /// </summary>
        public string HelpLink { get; set; }

        /// <summary>
        /// Optional highlighting data used to highlight a text in a window if the Fix it button is clicked.
        /// </summary>
        public HighlighterFocusData HighlighterFocus { get; set; }

        /// <summary>
        /// Defines parameters to highlight text in the editor from Project Validation.
        /// </summary>
        public struct HighlighterFocusData
        {
            /// <summary>
            /// Name of the window tab to highlight in.
            /// </summary>
            public string WindowTitle { get; set; }
            
            /// <summary>
            /// Text to highlight.
            /// </summary>
            public string SearchText { get; set; }
        }

        /// <summary>
        /// Whether to prevent this build rule from running when the Editor is in the
        /// [Prefab mode](xref:EditingInPrefabMode)).
        /// </summary>
        /// <remarks>
        /// Set this property to <see langword="true"/> when you have a rule that checks whether a Scene is setup correctly, but which
        /// fails in the special Editor Prefab mode. Set this property to <see langword="false"/> for other, general purpose rules.
        /// </remarks>
        public bool SceneOnlyValidation { get; set; }

        /// <summary>
        /// Lambda function that is invoked when the item is clicked in the validator.
        /// </summary>
        public Action OnClick { get; set; }

        /// <summary>
        /// Gets a string with the issue <see cref="Category"/> and <see cref="Message"/>.
        /// </summary>
        /// <returns>Returns a string with the issue <see cref="Category"/> and <see cref="Message"/>.</returns>
        public string GetDisplayString()
        {
            return string.IsNullOrEmpty(Category) ? Message : $"[{Category}] {Message}";
        }
    }
}
