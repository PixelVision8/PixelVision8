
using System;
using NUnit.Framework;

namespace PixelVision8.Player
{
    public class TestChip : AbstractChip, IUpdate, IDraw
    {

        public bool HasConfigured;
        public bool HasInitialized;
        public bool HasUpdated;
        public bool HasDrawn;
        public bool HasReset;
        public bool HasShutdown;
        
        public override void Init()
        {
            HasInitialized = true;
        }

        protected override void Configure()
        {
            HasConfigured = true;
        }
        
        public void Update(int timeDelta)
        {
            HasUpdated = true;
        }
        
        public void Draw()
        {
            HasDrawn = true;
        }

        public override void Shutdown()
        {
            HasShutdown = true;
        }

        public override void Reset()
        {
            HasReset = true;
        }

    }
    
    public partial class PixelVisionTest
    {

        [Test]
        public void EngineNameTest()
        {
            var player = new PixelVision(null, "TestPlayer");
            
            Assert.AreEqual(player.Name, "TestPlayer");
        }
        
        [Test]
        public void DefaultEngineNameTest()
        {

            var tmpPlayer = new PixelVision();
            
            Assert.AreEqual(tmpPlayer.Name, "Player");
        }
        
        [Test]
        public void ActivateChipTest()
        {

            var player = new PixelVision();

            var chipId = typeof(TestChip).FullName;
            
            player.ActivateChip(chipId, new TestChip());

            Assert.NotNull(player.GetChip(chipId));
            
        }
        
        [Test]
        public void HasChipTest()
        {

            var player = new PixelVision();

            var chipId = typeof(TestChip).FullName;
            
            player.ActivateChip(chipId, new TestChip());

            Assert.NotNull(player.HasChip(chipId));
            
        }
        
        [Test]
        public void GetChipTest()
        {

            var player = new PixelVision();
            
            var tmpChip = player.GetChip(typeof(AbstractChip).FullName);
            
            Assert.NotNull(tmpChip);
            
        }
        
        [Test]
        public void ChipConfigTest()
        {
            
            var player = new PixelVision();

            var chipId = typeof(TestChip).FullName;
            
            player.ActivateChip(chipId, new TestChip());
            
            var tmpChip = player.GetChip(typeof(TestChip).FullName) as TestChip;
            
            Assert.IsTrue(tmpChip != null && tmpChip.HasConfigured);
            
        }
        
        [Test]
        public void ChipInitTest()
        {
            
            var player = new PixelVision();

            var chipId = typeof(TestChip).FullName;
            
            player.ActivateChip(chipId, new TestChip());
            
            var tmpChip = player.GetChip(typeof(TestChip).FullName) as TestChip;
            
            player.RunGame();
            
            Assert.IsTrue(tmpChip != null && tmpChip.HasInitialized);
            
        }
        
        [Test]
        public void ChipUpdateTest()
        {
            
            var player = new PixelVision();

            var chipId = typeof(TestChip).FullName;
            
            player.ActivateChip(chipId, new TestChip());
            
            var tmpChip = player.GetChip(typeof(TestChip).FullName) as TestChip;
            
            player.RunGame();
            player.Update(0);
            
            Assert.IsTrue(tmpChip != null && tmpChip.HasUpdated);
            
        }
        
        [Test]
        public void ChipDrawTest()
        {
            
            var player = new PixelVision();

            var chipId = typeof(TestChip).FullName;
            
            player.ActivateChip(chipId, new TestChip());
            
            var tmpChip = player.GetChip(typeof(TestChip).FullName) as TestChip;
            
            player.RunGame();
            player.Draw();
            
            Assert.IsTrue(tmpChip != null && tmpChip.HasDrawn);
            
        }
        
        [Test]
        public void ChipResetTest()
        {
            
            var player = new PixelVision();

            var chipId = typeof(TestChip).FullName;
            
            player.ActivateChip(chipId, new TestChip());
            
            var tmpChip = player.GetChip(typeof(TestChip).FullName) as TestChip;
            
            player.RunGame();
            player.ResetGame();
            
            Assert.IsTrue(tmpChip != null && tmpChip.HasReset);
            
        }

    }
}