
using System;
using System.Linq;
using NUnit.Framework;

namespace PixelVision8.Player
{
    
    public class ColorChipTest
    {
        private PixelVision _pixelVision;
        private readonly string[] _defaultColors =
        {
            "#2D1B2E",
            "#218A91",
            "#3CC2FA",
            "#9AF6FD",
            "#4A247C",
            "#574B67",
            "#937AC5",
            "#8AE25D",
            "#8E2B45",
            "#F04156",
            "#F272CE",
            "#D3C0A8",
            "#C5754A",
            "#F2A759",
            "#F7DB53",
            "#F9F4EA"
        };
        
        [SetUp]
        public void Setup()
        {

            var chips = new[]
            {
                typeof(ColorChip).FullName
            };
            
            _pixelVision = new PixelVision(chips);
            
        }
        
        [Test]
        public void GetColorChipTest()
        {
            
            var tmpChip = _pixelVision.GetChip(typeof(ColorChip).FullName);
            
            Assert.NotNull(tmpChip);
            
        }
        
        [Test]
        public void ReadFromColorChipTest()
        {
            
            Assert.NotNull(_pixelVision.ColorChip);
            
        }

        #region Validate Hex Color

        /// <summary>
        ///     Test validating a hex color
        /// </summary>
        [Test]
        public void ValidateValidHexColor()
        {
            Assert.IsTrue(ColorChip.ValidateHexColor("#FFFFFF"));
        }
        
        /// <summary>
        ///     Test validating an invalid hex color
        /// </summary>
        [Test]
        public void ValidateInvalidHexColor()
        {
            Assert.IsFalse(ColorChip.ValidateHexColor("#ZZDWAF"));
        }
        
        /// <summary>
        ///     Test validating a 3 character hex color
        /// </summary>
        [Test]
        public void ValidateShortHexColor()
        {
            Assert.IsTrue(ColorChip.ValidateHexColor("#FFF"));
        }
        
        /// <summary>
        ///     Test validating a invalid 7 character hex color
        /// </summary>
        [Test]
        public void ValidateLongHexColor()
        {
            Assert.IsFalse(ColorChip.ValidateHexColor("#FFFFFFF"));
        }

        #endregion

        #region Background Color

        /// <summary>
        ///     Test the default background color value which should be -1
        /// </summary>
        [Test]
        public void BGDefaultValueTest()
        {
            Assert.AreEqual(-1, _pixelVision.ColorChip.BackgroundColor);
        }
        
        /// <summary>
        ///     Test setting a background color that is out of range
        /// </summary>
        [Test]
        public void BGOutBoundsTest()
        {

            _pixelVision.ColorChip.BackgroundColor = 200;

            Assert.AreEqual(-1, _pixelVision.ColorChip.BackgroundColor);

        }

        #endregion

        #region Mask Color

        /// <summary>
        ///     Test reading the default mask color
        /// </summary>
        [Test]
        public void MaskColorTest()
        {
            Assert.AreEqual("#FF00FF", _pixelVision.ColorChip.MaskColor);
        }
        
        /// <summary>
        ///     Test changing mask color
        /// </summary>
        [Test]
        public void ChangeMaskColorTest()
        {
            _pixelVision.ColorChip.MaskColor = "#FFFF00";
            
            Assert.AreEqual("#FFFF00", _pixelVision.ColorChip.MaskColor);
        }
        
        /// <summary>
        ///     Test changing mask to invalid color
        /// </summary>
        [Test]
        public void ChangeMaskToInvalidColorTest()
        {
            _pixelVision.ColorChip.MaskColor = "#FFFF0000";
            
            Assert.AreEqual("#FF00FF", _pixelVision.ColorChip.MaskColor);
        }
        
        #endregion

        #region Total Used Colors

        /// <summary>
        ///     Test the default number of colors after adding additional mask colors
        /// </summary>
        [Test]
        public void DefaultTotalUsedColorsTest()
        {
            Assert.AreEqual(_defaultColors.Length, _pixelVision.ColorChip.TotalUsedColors);
        }
        
        /// <summary>
        ///     Test the default number of colors after adding additional mask colors
        /// </summary>
        [Test]
        public void ResizeTotalUsedColorsTest()
        {

            _pixelVision.ColorChip.Total = 255;
            
            Assert.AreEqual(_defaultColors.Length, _pixelVision.ColorChip.TotalUsedColors);
        }
        
        /// <summary>
        ///     Test setting the total colors below 2
        /// </summary>
        [Test]
        public void ResizeLowTotalColorTest()
        {
            _pixelVision.ColorChip.Total = 1;
            Assert.AreEqual(2, _pixelVision.ColorChip.TotalUsedColors);
        }
        
        /// <summary>
        ///     Test the default number of colors after adding additional mask colors
        /// </summary>
        [Test] 
        public void TotalUsedColorsMaskTest()
        {
            var testColors = _defaultColors.ToList();

            for (int i = 0; i < 25; i++)
            {
                testColors.Add(_pixelVision.ColorChip.MaskColor);
            }

            _pixelVision.ColorChip.Total = testColors.Count;
            
            // the mask color counts as a color
            Assert.AreEqual(_defaultColors.Length, _pixelVision.ColorChip.TotalUsedColors);
        }

        #endregion


        #region Hex Colors

        
        /// <summary>
        ///     Test default hex colors
        /// </summary>
        [Test]
        public void DefaultHexColorTest()
        {
            CollectionAssert.AreEqual(_defaultColors, _pixelVision.ColorChip.HexColors);
        }
        
        /// <summary>
        ///     Test that additional colors are returned as mask colors
        /// </summary>
        [Test]
        public void AdditionalHexColorTest()
        {

            _pixelVision.ColorChip.Total = 20;

            var colors = _defaultColors.ToList();

            var total = _pixelVision.ColorChip.Total - _defaultColors.Length;
            
            for (int i = 0; i < total ; i++)
            {
                colors.Add(_pixelVision.ColorChip.MaskColor);
            }
            
            CollectionAssert.AreEqual(colors.ToArray(), _pixelVision.ColorChip.HexColors);
        }

        #endregion

        #region Total Colors

        /// <summary>
        ///     Check the default total colors
        /// </summary>
        [Test]
        public void DefaultTotalTest()
        {
            Assert.AreEqual(_defaultColors.Length, _pixelVision.ColorChip.Total);
        }
        
        /// <summary>
        ///     Test changing total colors to higher value
        /// </summary>
        [Test]
        public void NewTotalTest()
        {

            _pixelVision.ColorChip.Total = 255;
            
            Assert.AreEqual(255, _pixelVision.ColorChip.Total);
        }
        
        /// <summary>
        ///     Test changing total colors to invalid number that's too small
        /// </summary>
        [Test]
        public void NewInvalidTotalATest()
        {

            _pixelVision.ColorChip.Total = 1;
            
            Assert.AreEqual(2, _pixelVision.ColorChip.Total);
        }
        
        /// <summary>
        ///     Test changing total colors to negative number
        /// </summary>
        [Test]
        public void NewNegativeTotalATest()
        {

            _pixelVision.ColorChip.Total = -100;
            
            Assert.AreEqual(2, _pixelVision.ColorChip.Total);
        }

        #endregion

        #region Debug Mode

        /// <summary>
        ///     Test that default debug flag is set to false
        /// </summary>
        [Test]
        public void DefaultDebugColorTest()
        {
            
            Assert.IsFalse(_pixelVision.ColorChip.DebugMode);

        }
        
        /// <summary>
        ///     Test changing the debug color flag to true
        /// </summary>
        [Test]
        public void DebugColorTest()
        {
            _pixelVision.ColorChip.DebugMode = true;

            Assert.IsTrue(_pixelVision.ColorChip.DebugMode);

        }
        
        /// <summary>
        ///     Test that out of range colors still return the mask even with debug mode set to true
        /// </summary>
        [Test]
        public void DebugTrueColorTest()
        {
            _pixelVision.ColorChip.DebugMode = true;

            Assert.AreEqual(_pixelVision.ColorChip.MaskColor, _pixelVision.ColorChip.ReadColorAt(-1) );
            Assert.AreEqual(_pixelVision.ColorChip.MaskColor, _pixelVision.ColorChip.ReadColorAt(17) );

        }
        
        /// <summary>
        ///     Test that out of range colors still return the mask even with debug mode set to false
        /// </summary>
        [Test]
        public void DebugFalseColorTest()
        {
            _pixelVision.ColorChip.DebugMode = false;

            Assert.AreEqual(_pixelVision.ColorChip.MaskColor, _pixelVision.ColorChip.ReadColorAt(-1) );
            Assert.AreEqual(_pixelVision.ColorChip.MaskColor, _pixelVision.ColorChip.ReadColorAt(17) );

        }

        #endregion

        #region Invalidate Test

        /// <summary>
        ///     Test default invalid value is set to true
        /// </summary>
        [Test]
        public void DefaultInvalidTest()
        {
            Assert.IsTrue(_pixelVision.ColorChip.Invalid);
        }
        
        /// <summary>
        ///     Test default invalid value is set to true
        /// </summary>
        [Test]
        public void ResetInvalidTest()
        {
            _pixelVision.ColorChip.ResetValidation();
            
            Assert.IsFalse(_pixelVision.ColorChip.Invalid);
        }
        
        /// <summary>
        ///     Test invalid flag becomes true after updating color
        /// </summary>
        [Test]
        public void ChangeInvalidTest()
        {
            // Make sure the chip has reset the validation correctly
            _pixelVision.ColorChip.ResetValidation();
            Assert.IsFalse(_pixelVision.ColorChip.Invalid);
            
            _pixelVision.ColorChip.UpdateColorAt(0, "#FFFFFF");
            Assert.IsTrue(_pixelVision.ColorChip.Invalid);
        }
        
        /// <summary>
        ///     Test invalid flag isn't triggered on invalid hex
        /// </summary>
        [Test]
        public void ChangeValidationOutOfRangeTest()
        {
            // Make sure the chip has reset the validation correctly
            _pixelVision.ColorChip.ResetValidation();
            Assert.IsFalse(_pixelVision.ColorChip.Invalid);
            
            _pixelVision.ColorChip.UpdateColorAt(40, "#FFFFFF");
            Assert.IsFalse(_pixelVision.ColorChip.Invalid);
        }
        
        /// <summary>
        ///     Test invalid flag isn't triggered on invalid hex
        /// </summary>
        [Test]
        public void ChangeValidationInvalidHexTest()
        {
            // Make sure the chip has reset the validation correctly
            _pixelVision.ColorChip.ResetValidation();
            Assert.IsFalse(_pixelVision.ColorChip.Invalid);
            
            _pixelVision.ColorChip.UpdateColorAt(0, "#FFFFFF00");
            Assert.IsFalse(_pixelVision.ColorChip.Invalid);
        }
        
        /// <summary>
        ///     Test invalid flag isn't triggered when reading color
        /// </summary>
        [Test]
        public void NoValidationChangeOnReadTest()
        {
            // Make sure the chip has reset the validation correctly
            _pixelVision.ColorChip.ResetValidation();
            Assert.IsFalse(_pixelVision.ColorChip.Invalid);
            
            var color = _pixelVision.ColorChip.ReadColorAt(0);
            Assert.IsFalse(_pixelVision.ColorChip.Invalid);
        }
        
        /// <summary>
        ///     Test invalid flag isn't triggered when reading hex colors
        /// </summary>
        [Test]
        public void NoValidationChangeOnHexTest()
        {
            // Make sure the chip has reset the validation correctly
            _pixelVision.ColorChip.ResetValidation();
            Assert.IsFalse(_pixelVision.ColorChip.Invalid);
            
            var colors = _pixelVision.ColorChip.HexColors;
            Assert.IsFalse(_pixelVision.ColorChip.Invalid);
        }
        
        /// <summary>
        ///     Test invalid flag is set when changing total
        /// </summary>
        [Test]
        public void ChangeTotalInvalidTest()
        {
            // Make sure the chip has reset the validation correctly
            _pixelVision.ColorChip.ResetValidation();
            Assert.IsFalse(_pixelVision.ColorChip.Invalid);

            _pixelVision.ColorChip.Total = 255;
            Assert.IsTrue(_pixelVision.ColorChip.Invalid);
            
        }
        
        /// <summary>
        ///     Test invalid flag is set when changing to debug mode
        /// </summary>
        [Test]
        public void ChangeDebugInvalidTest()
        {
            // Make sure the chip has reset the validation correctly
            _pixelVision.ColorChip.ResetValidation();
            Assert.IsFalse(_pixelVision.ColorChip.Invalid);

            _pixelVision.ColorChip.DebugMode = true;
            Assert.IsTrue(_pixelVision.ColorChip.Invalid);
            
        }

        /// <summary>
        ///     Test invalid flag is set when changing background
        /// </summary>
        [Test]
        public void ChangeBackgroundInvalidTest()
        {
            // Make sure the chip has reset the validation correctly
            _pixelVision.ColorChip.ResetValidation();
            Assert.IsFalse(_pixelVision.ColorChip.Invalid);

            _pixelVision.ColorChip.BackgroundColor = 5;
            Assert.IsTrue(_pixelVision.ColorChip.Invalid);
            
        }
        
        /// <summary>
        ///     Test invalid flag is set when clearing color chip
        /// </summary>
        [Test]
        public void ChangeClearInvalidTest()
        {
            // Make sure the chip has reset the validation correctly
            _pixelVision.ColorChip.ResetValidation();
            Assert.IsFalse(_pixelVision.ColorChip.Invalid);

            _pixelVision.ColorChip.Clear();
            Assert.IsTrue(_pixelVision.ColorChip.Invalid);
            
        }
        
        /// <summary>
        ///     Testing that changing a color to the same value doesn't invalidate it
        /// </summary>
        [Test]
        public void ChangeSameColorInvalidTest()
        {
            
            _pixelVision.ColorChip.ResetValidation();
            Assert.IsFalse(_pixelVision.ColorChip.Invalid);
            
            _pixelVision.ColorChip.UpdateColorAt(0, _defaultColors[0]);
            
            // The mask color counts as a color
            Assert.IsFalse(_pixelVision.ColorChip.Invalid);
            
        }
        
        #endregion

        #region Read Color

        /// <summary>
        ///     Test reading the first color from the color chip
        /// </summary>
        [Test]
        public void ReadColorTest()
        {
            
            Assert.AreEqual(_defaultColors[0], _pixelVision.ColorChip.ReadColorAt(0));
            
        }
        
        /// <summary>
        ///     Test reading colors that are out of range which should always return the mask color
        /// </summary>
        [Test]
        public void ReadColorOutOfRangeTest()
        {
            Assert.AreEqual(_pixelVision.ColorChip.MaskColor, _pixelVision.ColorChip.ReadColorAt(-20));
            Assert.AreEqual(_pixelVision.ColorChip.MaskColor, _pixelVision.ColorChip.ReadColorAt(20));
        }

        #endregion

        #region Clear

        /// <summary>
        ///     Test reading colors that are out of range which should always return the mask color
        /// </summary>
        [Test]
        public void ClearDefaultTest()
        {
            
            _pixelVision.ColorChip.Clear();
            
            Assert.AreEqual(_pixelVision.ColorChip.MaskColor, _pixelVision.ColorChip.ReadColorAt(0));
        }
        
        /// <summary>
        ///     Test reading colors that are out of range which should always return the mask color
        /// </summary>
        [Test]
        public void ClearWithValueTest()
        {
            
            _pixelVision.ColorChip.Clear("#FFFF00");
            
            Assert.AreEqual("#FFFF00", _pixelVision.ColorChip.ReadColorAt(0));
        }
        
        #endregion


        #region Update Color

        /// <summary>
        ///     Testing updating a color that is in range
        /// </summary>
        [Test]
        public void AddColorTest()
        {
            
            _pixelVision.ColorChip.UpdateColorAt(0, "#FFFF00");
        
            // The mask color counts as a color
            Assert.AreEqual("#FFFF00", _pixelVision.ColorChip.ReadColorAt(0));
            
        }

        /// <summary>
        ///     Test that adding a color out of bounds doesn't change the total and returns the mask
        /// </summary>
        [Test]
        public void UpdateColorOutOfBoundsTest()
        {
            
            _pixelVision.ColorChip.UpdateColorAt(50, "#FFFFFF");
        
            // The mask color counts as a color
            Assert.AreEqual(_pixelVision.ColorChip.MaskColor, _pixelVision.ColorChip.ReadColorAt(55));
        
        }
        
        /// <summary>
        ///     Test that adding a color out of bounds doesn't change the total and returns the mask
        /// </summary>
        [Test]
        public void UpdateInvalidedColorTest()
        {
            
            _pixelVision.ColorChip.UpdateColorAt(0, "#FFFFFF00");
        
            // The mask color counts as a color
            Assert.AreEqual(_defaultColors[0], _pixelVision.ColorChip.ReadColorAt(0));
        
        }
        
        /// <summary>
        ///     Test that adding a color out of bounds doesn't change the total and returns the mask
        /// </summary>
        [Test]
        public void UpdateLowercaseColorTest()
        {
            
            _pixelVision.ColorChip.UpdateColorAt(0, "#aaff00");
        
            // The mask color counts as a color
            Assert.AreEqual("#AAFF00", _pixelVision.ColorChip.ReadColorAt(0));
        
        }
        
        /// <summary>
        ///     Test changing color after resize
        /// </summary>
        [Test]
        public void ResizeUpdateColorTest()
        {

            _pixelVision.ColorChip.Total = 20;
            
            _pixelVision.ColorChip.UpdateColorAt(19, "#FFFF00");
        
            // The mask color counts as a color
            Assert.AreEqual("#FFFF00", _pixelVision.ColorChip.ReadColorAt(19));
        
        }

        #endregion

    }
}