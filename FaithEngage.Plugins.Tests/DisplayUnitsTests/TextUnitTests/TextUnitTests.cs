﻿using System;
using NUnit.Framework;
using System.Collections.Generic;
using FaithEngage.Core.Exceptions;
using FakeItEasy;
using FaithEngage.Core.Cards;


namespace FaithEngage.Plugins.Tests
{

    [TestFixture]
    public class TextUnitTests
    {   
        private const string TEST_TEXT = "TEST TEXT";
        private Guid VALID_GUID = Guid.NewGuid();
        private Dictionary<string,string> loadedDict = new Dictionary<string, string>(){
            {"Name", "My Text Unit"},
            {"DateCreated", DateTime.Now.ToShortDateString()},
            {"Text", TEST_TEXT}
        };



        [Test]
        public void Ctor_ValidAttributes_NoId_Loaded()
        {
            var tu = new TextUnit (loadedDict);

            Assert.That (tu, Is.Not.Null);
            Assert.That (tu.Name == "My Text Unit");
            Assert.That (tu.Text == TEST_TEXT);
            Assert.That (tu.DateCreated.ToShortDateString () == DateTime.Now.ToShortDateString ());
            Assert.That (tu.Id != Guid.Empty);
            Assert.That (tu.Plugin, Is.InstanceOf (typeof(TextUnitPlugin)));
        }

        [Test]
        public void Ctor_NoAttributes_NoId()
        {
            var tu = new TextUnit (new Dictionary<string, string> ());

            Assert.That (tu, Is.Not.Null);
            Assert.That (tu.Id != Guid.Empty);
        }

        [Test]
        public void Ctor_LoadedAttributes_WithId()
        {
            var tu = new TextUnit (VALID_GUID, loadedDict);

            Assert.That (tu, Is.Not.Null);
            Assert.That (tu.Name == "My Text Unit");
            Assert.That (tu.Text == TEST_TEXT);
            Assert.That (tu.DateCreated.ToShortDateString () == DateTime.Now.ToShortDateString ());
            Assert.That (tu.Id == VALID_GUID);
            Assert.That (tu.Plugin, Is.InstanceOf (typeof(TextUnitPlugin)));
        }

        [Test]
        [ExpectedException(typeof(EmptyGuidException))]
        public void Ctor_LoadedAttributes_EmptyGuid()
        {
            var tu = new TextUnit (Guid.Empty, loadedDict);
        }

        [Test]
        public void GetAttributes_ReturnsTextProperty()
        {
            var tu = new TextUnit (loadedDict);
            var atts = tu.GetAttributes ();
            var text = atts ["Text"];

            Assert.That (atts, Is.Not.Null);
            Assert.That (text == TEST_TEXT);
        }

        [Test]
        public void GetCard_ReturnsFullyFunctionalTextCard()
        {
            var tu = new TextUnit (loadedDict) { 
                AssociatedEvent = VALID_GUID,
                DateCreated = DateTime.Now,
                Description = "My Lovely Description",
                PositionInEvent = 1
            };
            var card = tu.GetCard ();

            Assert.That (card, Is.Not.Null);
            Assert.That (card, Is.InstanceOf (typeof(TextCard)));
            Assert.That (card.Title == "My Text Unit");
            Assert.That (card.Description == "My Lovely Description");
            Assert.That (card.OriginatingDisplayUnit, Is.EqualTo (tu));
            Assert.That (card.Sections.Length == 1);
            Assert.That (card.Sections [0].HeadingText == card.Title);
            Assert.That (card.Sections [0].HtmlContents == TEST_TEXT);
        }

        [Test]
        public void ExecuteCardAction_ReturnsNull()
        {
            var tu = new TextUnit (loadedDict);
            var actionResult = tu.ExecuteCardAction (A.Dummy<CardAction> ());

            Assert.That (actionResult, Is.Null);
        }

        [Test]
        public void SetAttributes_HasTextAttribute_SetsTextProperty()
        {
            var tu = new TextUnit (loadedDict);
            var dict = new Dictionary<string,string> () {
                { "Text", "<p>This is my new text</p>" },
                { "Some Inapplicable Property", "The value to go with it" }
            };

            tu.SetAttributes (dict);

            Assert.That (tu.Text == "<p>This is my new text</p>");
        }

        [Test]
        public void Plugin_TextUnitPlugin()
        {
            var tu = new TextUnit (loadedDict);

            Assert.That (tu.Plugin, Is.Not.Null);
            Assert.That (tu.Plugin, Is.InstanceOf (typeof(TextUnitPlugin)));
        }

    }
}

