﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;

namespace ninepatch_generator
{
    enum StretchableImageDirection
    {
        Vertical = 0,
        Horizontal = 1
    }
    class StretchableImage
    {
        Dictionary<StretchableImageDirection, List<StretchableArea>> StretchableAreas;
        Bitmap Source;
        Bitmap CutSource;

        static public implicit operator string(StretchableImage lhs) {
            string result = "";

            result += string.Format(
                "Dimensions: {0}",
                lhs.CutSource.Size
            );
            result += "\n";

            result += string.Format(
                "Valid 9patch: {0}",
                lhs.IsValid().ToString()
            );
            result += "\n";

            result += string.Format(
                "Total StaticAreas: {0}",
                lhs.StaticArea.ToString()
            );
            result += "\n";

            result += string.Format(
                "Total DynamicAreas (H={0}, V={1}): {2}",
                lhs.StretchableAreas[StretchableImageDirection.Horizontal].Count,
                lhs.StretchableAreas[StretchableImageDirection.Vertical].Count,
                lhs.DynamicArea.ToString()
            );

            return result;
        }

        private Size staticArea;
        private Size dynamicArea;
        public Size StaticArea
        {
            get
            {
                return staticArea;
            }
        }
        public Size DynamicArea
        {
            get
            {
                return dynamicArea;
            }
        }
        public bool IsValid()
        {
            return (StretchableAreas[StretchableImageDirection.Horizontal].Count + StretchableAreas[StretchableImageDirection.Vertical].Count) > 0;
        }

        #region Constructor
        public StretchableImage(Image source)
        {
            StretchableAreas = new Dictionary<StretchableImageDirection, List<StretchableArea>>();
            StretchableAreas[StretchableImageDirection.Vertical] = new List<StretchableArea>();
            StretchableAreas[StretchableImageDirection.Horizontal] = new List<StretchableArea>();

            Source = new Bitmap(source);
            CutSource = new Bitmap(Source.Width - 2, Source.Height - 2);

            for (int y = 1; y < Source.Height - 1; y++)
            {
                for (int x = 1; x < Source.Width - 1; x++)
                {
                    CutSource.SetPixel(x-1, y-1, Source.GetPixel(x, y));
                }
            }

            GenerateAreas();
        }
        #endregion

        #region Internal functions
        // Dirty solution - optimize this to a more general function (don't seperate horizontal and vertical loop)
        private void CreatePatches()
        {
            int StartedAt;

            // Get horizontal patch pixels
            StartedAt = -1;
            for (int x = 0; x < Source.Width - 1; x++)
            {
                if (Source.GetPixel(x, 0).ToArgb() == Color.Black.ToArgb())
                {
                    if (StartedAt == -1)
                    {
                        StartedAt = x;
                    }
                }
                else
                {
                    if (StartedAt != -1)
                    {
                        StretchableAreas[StretchableImageDirection.Horizontal].Add(new StretchableArea(StartedAt, x - StartedAt, false));
                        StartedAt = -1;
                    }
                }
            }

            if (StartedAt != -1)
            {
                StretchableAreas[StretchableImageDirection.Horizontal].Add(new StretchableArea(StartedAt, (Source.Width - 1) - StartedAt, false));
                StartedAt = -1;
            }

            // Get vertical patch pixels
            StartedAt = -1;
            for (int y = 0; y < Source.Height - 1; y++)
            {
                if (Source.GetPixel(0, y).ToArgb() == Color.Black.ToArgb())
                {
                    if (StartedAt == -1)
                    {
                        StartedAt = y;
                    }
                }
                else
                {
                    if (StartedAt != -1)
                    {
                        StretchableAreas[StretchableImageDirection.Vertical].Add(new StretchableArea(StartedAt, y - StartedAt, false));
                        StartedAt = -1;
                    }
                }
            }

            if (StartedAt != -1)
            {
                StretchableAreas[StretchableImageDirection.Vertical].Add(new StretchableArea(StartedAt, (Source.Height - 1) - StartedAt, false));
                StartedAt = -1;
            }
        }
        private void GenerateAreas()
        {
            CreatePatches();

            // Calculate area sizes
            foreach (StretchableArea area in StretchableAreas[StretchableImageDirection.Horizontal])
            {
                dynamicArea.Width += area.Length;
            }

            foreach (StretchableArea area in StretchableAreas[StretchableImageDirection.Vertical])
            {
                dynamicArea.Height += area.Length;
            }
            staticArea = Source.Size - DynamicArea;
        }
        #endregion

        #region Public functions
        public Bitmap GenerateImage(Size newSize)
        {
            if (newSize.Width < StaticArea.Width && newSize.Height < StaticArea.Height)
            {
                throw new Exceptions.StretchableImage_TooSmallException();
            }

            Bitmap result = new Bitmap(newSize.Width, newSize.Height);
            Size NewDynamicSize = newSize - StaticArea;

            // @TODO: Calculate new dynamic-area sizes

            // @TODO: Create bitmaps based on dynamic areas

            // @TODO: Stretch available dynamic areas to fit new size
            
            return result;
        }
        #endregion
    }
}