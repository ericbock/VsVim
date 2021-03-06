﻿using System.ComponentModel.Composition;
using System.Windows.Media;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace Vim.UI.Wpf.Implementation
{
    [Export(typeof(EditorFormatDefinition))]
    [Name(Constants.IncrementalSearchTagName)]
    [UserVisible(true)]
    internal sealed class IncrementalSearchMarkerDefinition : MarkerFormatDefinition
    {
        internal IncrementalSearchMarkerDefinition()
        {
            this.DisplayName = "VsVim Incremental Search";
            this.Fill = new SolidColorBrush(Colors.LightBlue);
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [Name(Constants.HighlightIncrementalSearchTagName)]
    [UserVisible(true)]
    internal sealed class HighlightIncrementalSearchMarkerDefinition : MarkerFormatDefinition
    {
        internal HighlightIncrementalSearchMarkerDefinition()
        {
            this.DisplayName = "VsVim Highlight Incremental Search";
            this.Fill = new SolidColorBrush(Colors.LightBlue);
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [Name(EditorFormatDefinitionNames.BlockCaret)]
    [UserVisible(true)]
    internal sealed class BlockCaretMarkerDefinition : EditorFormatDefinition
    {
        internal BlockCaretMarkerDefinition()
        {
            this.DisplayName = "VsVim Block Caret";
            this.ForegroundColor = Colors.Black;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [Name(EditorFormatDefinitionNames.CommandMargin)]
    [UserVisible(true)]
    internal sealed class CommandMarginDefinition : EditorFormatDefinition
    {
        internal CommandMarginDefinition()
        {
            this.DisplayName = "VsVim Command Window";
            this.ForegroundColor = Colors.Black;
            this.BackgroundColor = Colors.White;
        }
    }
}
