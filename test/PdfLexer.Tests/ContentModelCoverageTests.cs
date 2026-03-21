using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;
using PdfLexer.Content;
using PdfLexer.Content.Model;
using PdfLexer.DOM;
using PdfLexer.Writing;
using PdfLexer.Fonts;

namespace PdfLexer.Tests
{
    public class ContentModelCoverageTests
    {
        [Fact]
        public void Flatten_PreservesOrder_AcrossNestedContainers()
        {
            var text1 = TextContent<double>.Create("1", Standard14Font.GetHelvetica(), 12, new PdfPoint<double>(0, 0));
            var text2 = TextContent<double>.Create("2", Standard14Font.GetHelvetica(), 12, new PdfPoint<double>(0, 0));
            var text3 = TextContent<double>.Create("3", Standard14Font.GetHelvetica(), 12, new PdfPoint<double>(0, 0));

            var innerGroup = new MarkedContentGroup<double>(new MarkedContent("Inner"))
            {
                GraphicsState = new GfxState<double>(),
                Children = { text2 }
            };

            var outerGroup = new MarkedContentGroup<double>(new MarkedContent("Outer"))
            {
                GraphicsState = new GfxState<double>(),
                Children = { text1, innerGroup, text3 }
            };

            var list = new List<IContentNode<double>> { outerGroup };
            var flat = list.Flatten().ToList();

            Assert.Equal(3, flat.Count);
            Assert.Same(text1, flat[0]);
            Assert.Same(text2, flat[1]);
            Assert.Same(text3, flat[2]);
        }

        [Fact]
        public void Flatten_DoesNotExpand_Forms()
        {
            var text1 = TextContent<double>.Create("1", Standard14Font.GetHelvetica(), 12, new PdfPoint<double>(0, 0));
            var form = new FormContent<double>
            {
                Stream = new XObjForm(),
                GraphicsState = new GfxState<double>()
            };

            var group = new MarkedContentGroup<double>(new MarkedContent("Group"))
            {
                GraphicsState = new GfxState<double>(),
                Children = { text1, form }
            };

            var list = new List<IContentNode<double>> { group };
            var flat = list.Flatten().ToList();

            Assert.Equal(2, flat.Count);
            Assert.Same(text1, flat[0]);
            Assert.Same(form, flat[1]);
        }

        [Fact]
        public void Shift_NodeList_ShiftsDescendantLeaves()
        {
            var text = TextContent<double>.Create("Test", Standard14Font.GetHelvetica(), 12, new PdfPoint<double>(10, 20));
            var group = new MarkedContentGroup<double>(new MarkedContent("Group"))
            {
                GraphicsState = new GfxState<double>(),
                Children = { text }
            };

            var list = new List<IContentNode<double>> { group };
            list.Shift(5.0, 10.0);

            // Shift currently only updates CTM in GraphicsState for some leaf types
            // Let's check where it moved.
            // For TextContent, we need to see how it affects output eventually.
            // But let's verify CTM for now.
            Assert.Equal(5.0, text.Segments[0].GraphicsState.CTM.E);
            Assert.Equal(10.0, text.Segments[0].GraphicsState.CTM.F);
        }

        [Fact]
        public void Shift_GroupList_ShiftsDescendantLeaves()
        {
            var text = TextContent<double>.Create("Test", Standard14Font.GetHelvetica(), 12, new PdfPoint<double>(10, 20));
            var group = new MarkedContentGroup<double>(new MarkedContent("Group"))
            {
                GraphicsState = new GfxState<double>(),
                Children = { text }
            };

            var list = new List<IContentGroup<double>> { group };
            list.Shift(5.0, 10.0);

            Assert.Equal(5.0, text.Segments[0].GraphicsState.CTM.E);
            Assert.Equal(10.0, text.Segments[0].GraphicsState.CTM.F);
        }

        [Fact]
        public void Shift_Preserves_FormBoundaryBehavior()
        {
            // Shifting content with forms only shifts the form placement, not parsed descendants
            var form = new FormContent<double>
            {
                Stream = new XObjForm(),
                GraphicsState = new GfxState<double> { CTM = GfxMatrix<double>.Identity with { E = 10, F = 20 } }
            };

            var list = new List<IContentNode<double>> { form };
            list.Shift(5.0, 10.0);

            Assert.Equal(15.0, form.GraphicsState.CTM.E);
            Assert.Equal(30.0, form.GraphicsState.CTM.F);
        }

        [Fact]
        public void GetContentModel_And_GetContentNodes_ReturnEquivalentTrees()
        {
            using var doc = PdfDocument.Create();
            var page = new PdfPage(new PdfDictionary());
            doc.Pages.Add(page);
            {
                using var ctx = page.GetWriter();
                ctx.MarkedContent(new MarkedContent("P"));
                ctx.Font(Standard14Font.GetHelvetica(), 12.0);
                ctx.Text("Hello").EndText();
                ctx.EndMarkedContent();
            }

            var model = page.GetContentModel();
            var nodes = page.GetContentNodes();

            Assert.Equal(model.Count, nodes.Count);
            Assert.IsType<MarkedContentGroup<double>>(model[0]);
            Assert.IsType<MarkedContentGroup<double>>(nodes[0]);
            
            var groupModel = (MarkedContentGroup<double>)model[0];
            var groupNodes = (MarkedContentGroup<double>)nodes[0];
            
            Assert.Equal(groupModel.Tag.Name.Value, groupNodes.Tag.Name.Value);
            Assert.Equal(groupModel.Children.Count, groupNodes.Children.Count);
        }

        [Fact]
        public void GetContentModel_RespectsFlattenFormsCompatibility()
        {
            using var doc = PdfDocument.Create();
            var page = new PdfPage(new PdfDictionary());
            doc.Pages.Add(page);
            {
                using var ctx = page.GetWriter();
                var formWriter = new FormWriter(100, 100);
                formWriter.Font(Standard14Font.GetHelvetica(), 12);
                formWriter.Text("Inside").EndText();
                var formObj = formWriter.Complete();
                ctx.Form(formObj);
            }

            var unflattened = page.GetContentModel(false);
            Assert.Single(unflattened);
            Assert.IsType<FormContent<double>>(unflattened[0]);

            var flattened = page.GetContentModel(true);
            Assert.Single(flattened);
            Assert.IsType<TextContent<double>>(flattened[0]);
        }

        [Fact]
        public void ContentWriter_AddContent_SingleNode_WritesCorrectly()
        {
            var text = TextContent<double>.Create("Hello", Standard14Font.GetHelvetica(), 12, new PdfPoint<double>(10, 20));
            using var doc = PdfDocument.Create();
            var page = new PdfPage(new PdfDictionary());
            doc.Pages.Add(page);
            {
                using var ctx = page.GetWriter();
                ctx.AddContent(text);
            }

            var model = page.GetContentModel();
            Assert.Single(model);
            Assert.IsType<TextContent<double>>(model[0]);
            var readText = (TextContent<double>)model[0];
            Assert.Equal("Hello", readText.Text);
            Assert.Equal(10.0, readText.LineMatrix.E);
            Assert.Equal(20.0, readText.LineMatrix.F);
        }

        [Fact]
        public void ContentWriter_AddContent_NodeList_WritesMixedTreeCorrectly()
        {
            var text1 = TextContent<double>.Create("One", Standard14Font.GetHelvetica(), 12, new PdfPoint<double>(10, 20));
            var text2 = TextContent<double>.Create("Two", Standard14Font.GetHelvetica(), 12, new PdfPoint<double>(10, 40));
            var group = new MarkedContentGroup<double>(new MarkedContent("G"))
            {
                GraphicsState = new GfxState<double>(),
                Children = { text2 }
            };

            var list = new List<IContentNode<double>> { text1, group };

            using var doc = PdfDocument.Create();
            var page = new PdfPage(new PdfDictionary());
            doc.Pages.Add(page);
            {
                using var ctx = page.GetWriter();
                ctx.AddContent(list);
            }

            var model = page.GetContentModel();
            Assert.Equal(2, model.Count);
            Assert.IsType<TextContent<double>>(model[0]);
            Assert.IsType<MarkedContentGroup<double>>(model[1]);
        }

        [Fact]
        public void MarkedContentGroup_BoundingBox_UnionsChildren()
        {
            var text1 = TextContent<double>.Create("1", Standard14Font.GetHelvetica(), 12, new PdfPoint<double>(0, 0));
            var text2 = TextContent<double>.Create("2", Standard14Font.GetHelvetica(), 12, new PdfPoint<double>(100, 100));
            
            var group = new MarkedContentGroup<double>(new MarkedContent("G"))
            {
                GraphicsState = new GfxState<double>(),
                Children = { text1, text2 }
            };

            var bb = group.GetBoundingBox();
            Assert.True(bb.LLx >= 0.0 && bb.LLx < 5.0);
            Assert.True(bb.LLy >= 0.0 && bb.LLy < 5.0);
            // Text 2 at 100,100. Font size 12. 
            // Exact width depends on font, but it's definitely > 100.
            Assert.True(bb.URx > 100);
            Assert.True(bb.URy > 100);
        }

        [Fact]
        public void MarkedContentGroup_Transform_PropagatesToDescendants()
        {
            var text = TextContent<double>.Create("1", Standard14Font.GetHelvetica(), 12, new PdfPoint<double>(10, 20));
            var group = new MarkedContentGroup<double>(new MarkedContent("G"))
            {
                GraphicsState = new GfxState<double>(),
                Children = { text }
            };

            group.Transform(new GfxMatrix<double>(1, 0, 0, 1, 5, 10));

            Assert.Equal(5.0, text.Segments[0].GraphicsState.CTM.E);
            Assert.Equal(10.0, text.Segments[0].GraphicsState.CTM.F);
        }

        [Fact]
        public void MarkedContentGroup_TransformInitial_PropagatesToDescendants()
        {
            var text = TextContent<double>.Create("1", Standard14Font.GetHelvetica(), 12, new PdfPoint<double>(10, 20));
            var group = new MarkedContentGroup<double>(new MarkedContent("G"))
            {
                GraphicsState = new GfxState<double>(),
                Children = { text }
            };

            group.TransformInitial(new GfxMatrix<double>(1, 0, 0, 1, 5, 10));

            Assert.Equal(5.0, text.Segments[0].GraphicsState.CTM.E);
            Assert.Equal(10.0, text.Segments[0].GraphicsState.CTM.F);
        }

        [Fact]
        public void MarkedContentGroup_ClipExcept_PropagatesToDescendants()
        {
            var text = TextContent<double>.Create("1", Standard14Font.GetHelvetica(), 12, new PdfPoint<double>(10, 20));
            var group = new MarkedContentGroup<double>(new MarkedContent("G"))
            {
                GraphicsState = new GfxState<double>(),
                Children = { text }
            };

            group.ClipExcept(new PdfRect<double>(0, 0, 100, 100));
            Assert.NotNull(text.Segments[0].GraphicsState.Clipping);
        }

        [Fact]
        public void MarkedContentGroup_CopyArea_PreservesContainerMetadata()
        {
            var text = TextContent<double>.Create("1", Standard14Font.GetHelvetica(), 12, new PdfPoint<double>(10, 20));
            var group = new MarkedContentGroup<double>(new MarkedContent("G"))
            {
                GraphicsState = new GfxState<double>(),
                Children = { text }
            };

            var copied = (MarkedContentGroup<double>)group.CopyArea(new PdfRect<double>(0, 0, 100, 100));
            Assert.NotNull(copied);
            Assert.Equal("G", copied.Tag.Name.Value);
            Assert.Single(copied.Children);
        }

        [Fact]
        public void FormContent_GetBoundingBox_UsesBBoxAndPlacement()
        {
            var form = new XObjForm();
            form.BBox = new PdfRectangle(new PdfArray { new PdfDoubleNumber(0.0), new PdfDoubleNumber(0.0), new PdfDoubleNumber(50.0), new PdfDoubleNumber(50.0) });
            
            var fc = new FormContent<double> {
                Stream = form.NativeObject,
                GraphicsState = new GfxState<double> {
                    CTM = new GfxMatrix<double>(1, 0, 0, 1, 10, 20)
                }
            };

            var bb = fc.GetBoundingBox();
            Assert.Equal(10.0, bb.LLx);
            Assert.Equal(20.0, bb.LLy);
            Assert.Equal(60.0, bb.URx);
            Assert.Equal(70.0, bb.URy);
        }

        [Fact]
        public void FormContent_GetBoundingBox_FallsBackWithoutBBox()
        {
            var form = new XObjForm();
            // No BBox set
            
            var fc = new FormContent<double> {
                Stream = form.NativeObject,
                GraphicsState = new GfxState<double> {
                    CTM = new GfxMatrix<double>(1, 0, 0, 1, 10, 20)
                }
            };

            var bb = fc.GetBoundingBox();
            // Default BBox is Letter size
            Assert.True(bb.URx > bb.LLx);
        }

        [Fact]
        public void ContentNodes_RoundTrip_MixedPage_PreservesShape()
        {
            using var doc = PdfDocument.Create();
            var page = new PdfPage(PageSize.LETTER);
            doc.Pages.Add(page);
            
            using (var writer = page.GetWriter())
            {
                writer.Font(Standard14Font.GetHelvetica(), 12)
                    .Text("Text").EndText();
                
                writer.Rect(10, 10, 50, 50).Fill();
                
                writer.MarkedContent(new MarkedContent("P"));
                writer.Text("Inside").EndText();
                writer.EndMarkedContent();

                var form = new FormWriter(100, 100);
                form.Rect(0, 0, 10, 10).Fill();
                writer.Form(form.Complete());
            }

            // Parse -> GetContentNodes
            var nodes = page.GetContentNodes<double>();
            Assert.Equal(4, nodes.Count);
            Assert.IsType<TextContent<double>>(nodes[0]);
            Assert.IsType<PathSequence<double>>(nodes[1]);
            Assert.IsType<MarkedContentGroup<double>>(nodes[2]);
            Assert.IsType<FormContent<double>>(nodes[3]);

            // Write back to a new page
            var page2 = new PdfPage(PageSize.LETTER);
            doc.Pages.Add(page2);
            using (var writer2 = page2.GetWriter())
            {
                writer2.AddContent(nodes);
            }

            // Parse again and verify
            var nodes2 = page2.GetContentNodes<double>();
            Assert.Equal(4, nodes2.Count);
            Assert.IsType<TextContent<double>>(nodes2[0]);
            Assert.IsType<PathSequence<double>>(nodes2[1]);
            Assert.IsType<MarkedContentGroup<double>>(nodes2[2]);
            Assert.IsType<FormContent<double>>(nodes2[3]);
            
            Assert.Equal("Text", ((TextContent<double>)nodes2[0]).Text);
            Assert.Equal("P", ((MarkedContentGroup<double>)nodes2[2]).Tag.Name.Value);
        }

        [Fact]
        public void PdfDocument_Context_TracksAmbientParsingContext()
        {
            var ctx1 = new ParsingContext(null, true);
            using var doc = PdfDocument.Create();
            Assert.Same(ctx1, doc.Context);

            var ctx2 = new ParsingContext(null, true);
            Assert.Same(ctx2, doc.Context);
            
            ParsingContext.Reset();
        }

        [Fact]
        public void PdfDocument_Context_DoesNotImplyDocumentOwnedState()
        {
            var ctx1 = new ParsingContext(null, true);
            using var doc = PdfDocument.Create();
            var firstCtx = doc.Context;
            
            var ctx2 = new ParsingContext(null, true);
            // doc.Context should now reflect ctx2 because it just returns ParsingContext.Current
            Assert.NotSame(firstCtx, doc.Context);
            Assert.Same(ctx2, doc.Context);
            
            ParsingContext.Reset();
        }

        [Fact]
        public void PdfPage_ReadAccess_DocumentsCurrentMaterializationBehavior()
        {
            var page = new PdfPage(new PdfDictionary());
            Assert.False(page.NativeObject.ContainsKey(PdfName.Resources));
            Assert.False(page.NativeObject.ContainsKey(PdfName.MediaBox));
            Assert.False(page.NativeObject.ContainsKey(PdfName.Rotate));

            // Accessing properties materializes them
            var res = page.Resources;
            var mb = page.MediaBox;
            var rot = page.Rotate;

            Assert.True(page.NativeObject.ContainsKey(PdfName.Resources));
            Assert.True(page.NativeObject.ContainsKey(PdfName.MediaBox));
            Assert.True(page.NativeObject.ContainsKey(PdfName.Rotate));
        }

        [Fact]
        public void Save_Removes_Names_And_Encrypt_FromOutput()
        {
            using var doc = PdfDocument.Create();
            doc.Catalog[PdfName.Names] = new PdfDictionary();
            doc.Trailer[PdfName.Encrypt] = new PdfDictionary();
            
            using var ms = new MemoryStream();
            doc.SaveTo(ms);
            
            using var doc2 = PdfDocument.Open(ms.ToArray());
            Assert.False(doc2.Catalog.ContainsKey(PdfName.Names));
            Assert.False(doc2.Trailer.ContainsKey(PdfName.Encrypt));
        }

        [Fact]
        public void Save_Rebuilds_PageTree_For_CurrentPagesList()
        {
            using var doc = PdfDocument.Create();
            doc.Pages.Add(new PdfPage(PageSize.LETTER));
            doc.Pages.Add(new PdfPage(PageSize.A4));
            
            // Swap pages
            var p1 = doc.Pages[0];
            doc.Pages[0] = doc.Pages[1];
            doc.Pages[1] = p1;
            
            using var ms = new MemoryStream();
            doc.SaveTo(ms);
            
            using var doc2 = PdfDocument.Open(ms.ToArray());
            Assert.Equal(2, doc2.Pages.Count);
            // Verify sizes swapped
            var a4Box = new PdfRectangle(PageSizeHelpers.GetMediaBox(PageSize.A4));
            var letterBox = new PdfRectangle(PageSizeHelpers.GetMediaBox(PageSize.LETTER));
            Assert.Equal((decimal)a4Box.URx, (decimal)doc2.Pages[0].MediaBox.URx);
            Assert.Equal((decimal)letterBox.URx, (decimal)doc2.Pages[1].MediaBox.URx);
        }

        [Fact]
        public void PdfDocument_Outlines_IsLazyAndCached()
        {
            using var doc = PdfDocument.Create();
            var outlines1 = doc.Outlines;
            var outlines2 = doc.Outlines;
            Assert.Same(outlines1, outlines2);
        }

        [Fact]
        public void PdfDocument_Structure_LazyCreatesBuilder()
        {
            using var doc = PdfDocument.Create();
            var struct1 = doc.Structure;
            Assert.NotNull(struct1);
            var struct2 = doc.Structure;
            Assert.Same(struct1, struct2);
        }

        [Fact]
        public void RemoveUnusedResources_RemovesUnusedTopLevelFontAndXObject()
        {
            var page = new PdfPage(PageSize.LETTER);
            var font = Standard14Font.GetHelvetica();
            page.Resources.GetOrCreateValue<PdfDictionary>(PdfName.Font)[new PdfName("F1")] = font.GetPdfFont();
            
            var xobj = new XObjForm();
            page.Resources.GetOrCreateValue<PdfDictionary>(PdfName.XObject)[new PdfName("X1")] = xobj.NativeObject;
            
            // Neither F1 nor X1 is used in content
            page.RemoveUnusedResources();
            
            Assert.False(page.Resources.GetOrCreateValue<PdfDictionary>(PdfName.Font).ContainsKey(new PdfName("F1")));
            Assert.False(page.Resources.GetOrCreateValue<PdfDictionary>(PdfName.XObject).ContainsKey(new PdfName("X1")));
        }

        [Fact]
        public void RemoveUnusedResources_PreservesResourcesNeededByNestedForms()
        {
            var page = new PdfPage(PageSize.LETTER);
            var font = Standard14Font.GetHelvetica();
            page.Resources.GetOrCreateValue<PdfDictionary>(PdfName.Font)[new PdfName("F1")] = font.GetPdfFont();
            
            // Create a form that uses F1 (from page resources)
            var form = new XObjForm();
            form.NativeObject.Contents = new PdfByteArrayStreamContents(System.Text.Encoding.ASCII.GetBytes("/F1 12 Tf (Hi) Tj"));
            
            var formName = new PdfName("Form1");
            page.Resources.GetOrCreateValue<PdfDictionary>(PdfName.XObject)[formName] = form.NativeObject;
            
            // Use Form1 in page content manually to ensure name matches
            using (var writer = page.GetWriter())
            {
                // We need to use the name we put in resources
                writer.Op(new Do_Op(formName));
            }
            
            // Remove unused resources. F1 is used by Form1, so it should be preserved if it's in page resources.
            page.RemoveUnusedResources();
            
            Assert.True(page.Resources.GetOrCreateValue<PdfDictionary>(PdfName.Font).ContainsKey(new PdfName("F1")));
            Assert.True(page.Resources.GetOrCreateValue<PdfDictionary>(PdfName.XObject).ContainsKey(new PdfName("Form1")));
        }

        [Fact]
        public void DeduplicateResources_HandlesEquivalentResourceDictionaries()
        {
            using var doc = PdfDocument.Create();
            var page1 = doc.AddPage(PageSize.LETTER);
            var page2 = doc.AddPage(PageSize.LETTER);
            
            // Add identical fonts to both pages but as separate objects
            var font1 = Standard14Font.GetHelvetica();
            var font2 = Standard14Font.GetHelvetica();
            
            page1.Resources.GetOrCreateValue<PdfDictionary>(PdfName.Font)[new PdfName("F1")] = font1.GetPdfFont().Indirect();
            page2.Resources.GetOrCreateValue<PdfDictionary>(PdfName.Font)[new PdfName("F1")] = font2.GetPdfFont().Indirect();
            
            doc.DeduplicateResources();
            
            var f1 = page1.Resources.Get<PdfDictionary>(PdfName.Font)![new PdfName("F1")];
            var f2 = page2.Resources.Get<PdfDictionary>(PdfName.Font)![new PdfName("F1")];
            
            Assert.Equal(f1, f2); // Should be the same indirect reference
            Assert.Same(f1.Resolve(), f2.Resolve());
        }

        [Fact]
        public void DeduplicateResources_PreservesNestedFormSemantics()
        {
            using var doc = PdfDocument.Create();
            var page = doc.AddPage(PageSize.LETTER);
            
            // Create two identical forms
            var f1 = new FormWriter(100, 100);
            f1.Rect(0, 0, 10, 10).Fill();
            var s1 = f1.Complete();
            
            var f2 = new FormWriter(100, 100);
            f2.Rect(0, 0, 10, 10).Fill();
            var s2 = f2.Complete();
            
            page.Resources.GetOrCreateValue<PdfDictionary>(PdfName.XObject)[new PdfName("X1")] = s1.NativeObject.Indirect();
            page.Resources.GetOrCreateValue<PdfDictionary>(PdfName.XObject)[new PdfName("X2")] = s2.NativeObject.Indirect();
            
            doc.DeduplicateResources();
            
            var x1 = page.Resources.Get<PdfDictionary>(PdfName.XObject)![new PdfName("X1")];
            var x2 = page.Resources.Get<PdfDictionary>(PdfName.XObject)![new PdfName("X2")];
            
            Assert.Equal(x1, x2);
        }

        [Fact]
        public void PageToForm_UninheritResources_CopiesInheritedResourcesIntoNestedForms()
        {
            var page = new PdfPage(PageSize.LETTER);
            var font = Standard14Font.GetHelvetica();
            page.Resources.GetOrCreateValue<PdfDictionary>(PdfName.Font)[new PdfName("F1")] = font.GetPdfFont();
            
            // Create a form that uses F1 (from page resources) and doesn't have it in its own resources
            var innerForm = new XObjForm();
            innerForm.NativeObject.Contents = new PdfByteArrayStreamContents(System.Text.Encoding.ASCII.GetBytes("/F1 12 Tf (Hi) Tj"));
            innerForm.Resources = new PdfDictionary(); // Add empty resources
            
            page.Resources.GetOrCreateValue<PdfDictionary>(PdfName.XObject)[new PdfName("InnerForm")] = innerForm.NativeObject;
            
            // Use InnerForm in page content
            using (var writer = page.GetWriter())
            {
                writer.Form(innerForm);
            }
            
            // Convert page to form with uninheritResources = true
            var outerForm = XObjForm.FromPage(page, true);
            
            // The inner form inside outerForm should now have F1 in its resources
            var innerFormRef = outerForm.Resources!.Get<PdfDictionary>(PdfName.XObject)![new PdfName("InnerForm")];
            var innerFormObj = (PdfStream)innerFormRef.Resolve();
            var innerFormRes = innerFormObj.Dictionary.Get<PdfDictionary>(PdfName.Resources);
            
            Assert.NotNull(innerFormRes);
            Assert.True(innerFormRes.GetOrCreateValue<PdfDictionary>(PdfName.Font).ContainsKey(new PdfName("F1")));
        }

        [Fact]
        public void EncryptedInput_Save_RemovesEncryptAndProducesReadableOutput()
        {
            var tp = PathUtil.GetPathFromSegmentOfCurrent("test");
            var pdfRoot = Path.Combine(tp, "pdfs", "pdfrs");
            var pdf = Path.Combine(pdfRoot, "passwords_aes_128.pdf");
            if (!File.Exists(pdf)) return;

            using var doc = PdfDocument.Open(File.ReadAllBytes(pdf), new DocumentOptions
            {
                UserPass = "userpassword"
            });
            // Assert.True(doc.IsEncrypted); // doc.IsEncrypted is internal? 
            // Let's check doc.Trailer
            Assert.True(doc.Trailer.ContainsKey(PdfName.Encrypt));
            
            using var ms = new MemoryStream();
            doc.SaveTo(ms);
            
            using var doc2 = PdfDocument.Open(ms.ToArray());
            Assert.False(doc2.Trailer.ContainsKey(PdfName.Encrypt));
            
            // Verify content is readable
            var pg = doc2.Pages.First();
            var words = SimpleWordScanner.GetWords(doc2.Context, pg);
            Assert.Contains("Hello", words);
        }

        [Fact]
        public void CloneShallow_PageMutation_DoesNotBackPropagateUnexpectedly()
        {
            var page1 = new PdfPage(new PdfDictionary());
            var page2 = new PdfPage(page1.NativeObject.CloneShallow());
            
            page2.Rotate = 90;
            Assert.Equal(90, (int)page2.Rotate!);
            // Use ContainsKey to avoid getter materialization
            Assert.False(page1.NativeObject.ContainsKey(PdfName.Rotate));
        }

        [Fact]
        public void CrossDocumentPageReuse_RemainsValidUntilSaveBoundary()
        {
            using var doc1 = PdfDocument.Create();
            var page = doc1.AddPage(PageSize.LETTER);
            using (var writer = page.GetWriter())
            {
                writer.Font(Standard14Font.GetHelvetica(), 12).Text("Shared").EndText();
            }

            using var doc2 = PdfDocument.Create();
            // Add page from doc1 to doc2
            doc2.Pages.Add(page);
            
            using var ms = new MemoryStream();
            doc2.SaveTo(ms);
            
            using var doc3 = PdfDocument.Open(ms.ToArray());
            Assert.Single(doc3.Pages);
            var words = SimpleWordScanner.GetWords(doc3.Context, doc3.Pages[0]);
            Assert.Contains("Shared", words);
        }

        [Fact]
        public void ScannerAndContentModel_AgreeOnSimplePageTextAfterRewrite()
        {
            using var doc = PdfDocument.Create();
            var page = doc.AddPage(PageSize.LETTER);
            using (var writer = page.GetWriter())
            {
                writer.Font(Standard14Font.GetHelvetica(), 12).Text("Original").EndText();
            }

            // Rewrite using ContentModel
            var nodes = page.GetContentNodes<double>();
            var text = (TextContent<double>)nodes[0];
            
            // Change text to "Modified"
            text.Segments[0].Glyphs.Clear();
            foreach (var g in Standard14Font.GetHelvetica().GetGlyphs("Modified"))
            {
                text.Segments[0].Glyphs.Add(new GlyphOrShift<double>(g.Glyph, 0, g.Bytes));
            }
            
            using (var writer2 = page.GetWriter(PageWriteMode.Replace))
            {
                writer2.AddContent(nodes);
            }

            // Verify with Scanner
            var words = SimpleWordScanner.GetWords(doc.Context, page);
            Assert.Contains("Modified", words);
            Assert.DoesNotContain("Original", words);
        }

        [Fact]
        public void ScannerAndContentModel_AgreeOnNestedFormTraversal()
        {
            using var doc = PdfDocument.Create();
            var page = doc.AddPage(PageSize.LETTER);
            
            var formWriter = new FormWriter(100, 100);
            formWriter.Font(Standard14Font.GetHelvetica(), 12).Text("Inside").EndText();
            var form = formWriter.Complete();
            
            using (var writer = page.GetWriter())
            {
                writer.Form(form);
            }

            // Content Model check
            var nodes = page.GetContentNodes<double>(flattenForms: true);
            Assert.Contains(nodes.OfType<TextContent<double>>(), tc => tc.Text == "Inside");

            // Scanner check
            var words = SimpleWordScanner.GetWords(doc.Context, page);
            Assert.Contains("Inside", words);
        }
    }
}
