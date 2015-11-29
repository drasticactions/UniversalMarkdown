﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniversalMarkdown.Helpers;

namespace UniversalMarkdown.Parse.Elements
{
    class RawSubredditInline : MarkdownInline
    {
        public string Text { get; set; }

        public RawSubredditInline()
            : base(MarkdownInlineType.RawSubreddit)
        { }

        internal override int Parse(ref string markdown, int startingPos, int endingPos)
        {
            // Find the start of the subreddit link
            int subredditStart = Common.IndexOf(ref markdown, "r/", startingPos, endingPos);
            if(subredditStart != startingPos && (subredditStart - 1) != startingPos)
            {
                DebuggingReporter.ReportCriticalError("Trying to parse a subreddit link but didn't find a subreddit");
                return endingPos;
            }

            // Grab where to begin looking for the end.
            int subredditEnd = subredditStart + 2;
            int subredditTextStart = subredditStart + 2;

            // Check if there is a / before it, if so include it
            if (subredditStart != 0 && markdown[subredditStart - 1] == '/')
            {
                subredditStart--;
            }

            // While we didn't hit the end && (it is a char or digit or _ )
            while (subredditEnd < markdown.Length && subredditStart < endingPos && (Char.IsLetterOrDigit(markdown[subredditEnd]) || markdown[subredditEnd] == '_'))
            {
                subredditEnd++;
            }

            // Validate
            if(subredditEnd != endingPos)
            {
                DebuggingReporter.ReportCriticalError("Raw subreddit ending didn't match endingPos");
            }

            // Grab the text
            Text = markdown.Substring(subredditStart, subredditEnd - subredditStart);     

            // Return what we consumed
            return subredditEnd;
        }

        /// <summary>
        /// Attempts to find a element in the range given. If an element is found we must check if the starting is less than currentNextElementStart,
        /// and if so update that value to be the start and update the elementEndPos to be the end of the element. These two vales will be passed back to us
        /// when we are asked to parse. We then return true or false to indicate if we are the new candidate. 
        /// </summary>
        /// <param name="markdown">mark down to parse</param>
        /// <param name="currentPos">the starting point to search</param>
        /// <param name="maxEndingPos">the ending point to search</param>
        /// <param name="elementStartingPos">the current starting element, if this element is < we will update this to be our starting pos</param>
        /// <param name="elementEndingPos">The ending pos of this element if it is interesting.</param>
        /// <returns>true if we are the next element candidate, false otherwise.</returns>
        public static bool FindNextClosest(ref string markdown, int startingPos, int endingPos, ref int currentNextElementStart, ref int elementEndingPos)
        {
            // Test for raw subreddit links. We need to loop here so if we find a false positive
            // we can keep checking before the current closest. Note this logic must match the logic
            // in the subreddit link parser below.
            int subredditStart = Common.IndexOf(ref markdown, "r/", startingPos, endingPos);
            while (subredditStart != -1 && subredditStart < currentNextElementStart)
            {
                // Make sure the char before the r/ is not a letter
                if (subredditStart == 0 || !Char.IsLetterOrDigit(markdown[subredditStart - 1]))
                {
                    // Make sure there is something after the r/
                    if (subredditStart + 2 < markdown.Length && subredditStart + 2 < endingPos && Char.IsLetterOrDigit(markdown[subredditStart + 2]))
                    {
                        // Check if there is a / before it, if so include it
                        if (subredditStart != 0 && markdown[subredditStart - 1] == '/')
                        {
                            subredditStart--;
                        }

                        // Send the info off!
                        currentNextElementStart = subredditStart;
                        elementEndingPos = Common.FindNextWhiteSpace(ref markdown, currentNextElementStart, endingPos, true);
                        return true;
                    }
                }
                subredditStart += 2;
                subredditStart = Common.IndexOf(ref markdown, "r/", subredditStart, endingPos);
            }
            return false;
        }
    }
}