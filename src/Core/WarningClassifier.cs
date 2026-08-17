using System;

namespace ModelDoctor.Core
{
    /// <summary>
    /// Helper to classify Revit failure description text into granular audit categories.
    /// </summary>
    public static class WarningClassifier
    {
        /// <summary>
        /// Classifies a Revit warning description text into a category and rule name pair.
        /// </summary>
        public static (string Category, string RuleName) Classify(string descriptionText)
        {
            if (string.IsNullOrWhiteSpace(descriptionText))
                return ("Warnings - General", "Unclassified Model Warnings");

            string text = descriptionText.ToLowerInvariant();

            // 1. Curtain Walls & System Panels (Architectural)
            if (text.Contains("curtain") || text.Contains("system panel") || text.Contains("mullion"))
            {
                return ("Warnings - Geometry", "Curtain Wall & System Panel Issues");
            }

            // 2. Hosting & Attachments
            if (text.Contains("host") || text.Contains("orphan") || text.Contains("attached") || 
                text.Contains("attachment") || text.Contains("can't cut"))
            {
                return ("Warnings - Hosting", "Unhosted / Lost Host Elements");
            }

            // 3. MEP Systems (Mechanical, Electrical, Plumbing)
            if (text.Contains("mep") || text.Contains("pipe") || text.Contains("piping") || 
                text.Contains("duct") || text.Contains("conduit") || text.Contains("cable tray") ||
                text.Contains("circuit") || text.Contains("wire") || text.Contains("wiring") || 
                text.Contains("hvac") || text.Contains("plumbing") || text.Contains("mechanical") ||
                text.Contains("terminal") || text.Contains("electrical") ||
                (text.Contains("connect") && (text.Contains("fitting") || text.Contains("connector") || text.Contains("flow") || text.Contains("sanitary") || text.Contains("hydronic"))))
            {
                return ("Warnings - MEP Systems", "Unconnected MEP Components");
            }

            // 4. Geometry & Overlaps
            if (text.Contains("overlap") || text.Contains("duplicate") || text.Contains("same place") || 
                text.Contains("identical") || text.Contains("collision") || text.Contains("intersection"))
            {
                return ("Warnings - Geometry", "Overlapping & Duplicate Elements");
            }

            // 5. Rooms & Spaces
            if (text.Contains("room") || text.Contains("space") || text.Contains("enclosed") || 
                text.Contains("boundary") || text.Contains("area tag") || text.Contains("redundant"))
            {
                return ("Warnings - Rooms & Spaces", "Unenclosed / Redundant Rooms");
            }

            // 6. Structural
            if (text.Contains("analytical") || text.Contains("rebar") || text.Contains("structural") || 
                text.Contains("framing"))
            {
                return ("Warnings - Structural", "Structural & Analytical Issues");
            }

            return ("Warnings - General", "Other Revit Model Warnings");
        }
    }
}
