using PdfLexer.Content;
using PdfLexer.Content.Model;
using PdfLexer.DOM;
using PdfLexer.Fonts;
using PdfLexer.Operators;
using PdfLexer.Writing;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace PdfLexer.Tests
{
    public class CachedContentMutationTests
    {
        [Fact]
        public void Cache_Should_Reuse_Rewritten_Forms_For_Same_Transform()
        {
            // 1. Create a form
            var f1 = new FormWriter(PageSize.LETTER);
            f1.Rect(0, 0, 10, 10).Fill();
            var formStream = f1.Complete();

            // 2. Create a page with two identical references and one different
            var pg = new PdfPage(PageSize.LETTER);
            using (var writer = pg.GetWriter())
            {
                writer.Save()
                    .Translate(100, 100)
                    .Form(formStream)
                    .Restore()
                    .Save()
                    .Translate(100, 100) // Same transform
                    .Form(formStream)
                    .Restore()
                    .Save()
                    .Translate(200, 200) // Different transform
                    .Form(formStream)
                    .Restore();
            }

            // 3. Apply a mutation
            var mutation = new CachedContentMutation(node => {
                return node;
            });

            var pg2 = mutation.Apply(pg);

            // 4. Verify results
            var nodes = pg2.GetContentNodes<double>();
            var forms = nodes.OfType<FormContent<double>>().ToList();

            Assert.Equal(3, forms.Count);
            
            // Check if first two use the same stream
            Assert.Same(forms[0].Stream, forms[1].Stream);
            
            // Check if third uses a different stream
            Assert.NotSame(forms[0].Stream, forms[2].Stream);
        }

        [Fact]
        public void Cache_Should_Not_Reuse_Rewritten_Forms_For_Different_Clipping()
        {
            var f1 = new FormWriter(PageSize.LETTER);
            f1.Rect(0, 0, 10, 10).Fill();
            var formStream = f1.Complete();

            var pg = new PdfPage(PageSize.LETTER);
            using (var writer = pg.GetWriter())
            {
                writer.Save()
                    .Rect(new PdfRect<double> { LLx = 0, LLy = 0, URx = 50, URy = 50 })
                    .Clip(false)
                    .EndPathNoOp()
                    .Translate(100, 100)
                    .Form(formStream)
                    .Restore()
                    .Save()
                    .Rect(new PdfRect<double> { LLx = 0, LLy = 0, URx = 150, URy = 150 })
                    .Clip(false)
                    .EndPathNoOp()
                    .Translate(100, 100)
                    .Form(formStream)
                    .Restore();
            }

            var mutation = new CachedContentMutation(node => node);

            var pg2 = mutation.Apply(pg);

            var forms = pg2.GetContentNodes<double>().OfType<FormContent<double>>().ToList();
            Assert.Equal(2, forms.Count);
            Assert.NotSame(forms[0].Stream, forms[1].Stream);
        }

        [Fact]
        public void Mutation_Removes_EmptyMarkedContentContainers()
        {
            var pg = new PdfPage(PageSize.LETTER);
            using (var writer = pg.GetWriter())
            {
                writer.MarkedContent(new MarkedContent("P"));
                writer.Font(Standard14Font.GetHelvetica(), 12);
                writer.Text("RemoveMe").EndText();
                writer.EndMarkedContent();
            }

            var mutation = new CachedContentMutation(node =>
            {
                if (node is TextContent<double> tc && tc.Text == "RemoveMe")
                {
                    return null;
                }
                return node;
            });

            var pg2 = mutation.Apply(pg);
            var nodes = pg2.GetContentNodes<double>();
            Assert.Empty(nodes);
        }

        [Fact]
        public void Mutation_Removes_TransitivelyEmptyMarkedContentAncestors()
        {
            var pg = new PdfPage(PageSize.LETTER);
            using (var writer = pg.GetWriter())
            {
                writer.MarkedContent(new MarkedContent("Outer"));
                writer.MarkedContent(new MarkedContent("Inner"));
                writer.Font(Standard14Font.GetHelvetica(), 12);
                writer.Text("RemoveMe").EndText();
                writer.EndMarkedContent();
                writer.EndMarkedContent();
            }

            var mutation = new CachedContentMutation(node =>
            {
                if (node is TextContent<double> tc && tc.Text == "RemoveMe")
                {
                    return null;
                }
                return node;
            });

            var pg2 = mutation.Apply(pg);
            var nodes = pg2.GetContentNodes<double>();
            Assert.Empty(nodes);
        }

        [Fact]
        public void Mutation_Removes_Form_When_All_Descendants_Removed()
        {
            var f1 = new FormWriter(PageSize.LETTER);
            f1.Font(Standard14Font.GetHelvetica(), 12);
            f1.Text("RemoveMe").EndText();
            var formStream = f1.Complete();

            var pg = new PdfPage(PageSize.LETTER);
            using (var writer = pg.GetWriter())
            {
                writer.Form(formStream);
            }

            var mutation = new CachedContentMutation(node =>
            {
                if (node is TextContent<double> tc && tc.Text == "RemoveMe")
                {
                    return null;
                }
                return node;
            });

            var pg2 = mutation.Apply(pg);
            var nodes = pg2.GetContentNodes<double>();
            Assert.Empty(nodes);
        }

        [Fact]
        public void Cache_Reuses_Null_Result_For_EmptyFormRewrite()
        {
             var f1 = new FormWriter(PageSize.LETTER);
            f1.Font(Standard14Font.GetHelvetica(), 12);
            f1.Text("RemoveMe").EndText();
            var formStream = f1.Complete();

            var pg = new PdfPage(PageSize.LETTER);
            using (var writer = pg.GetWriter())
            {
                writer.Save().Translate(10, 10).Form(formStream).Restore();
                writer.Save().Translate(10, 10).Form(formStream).Restore();
            }

            int rewriteCount = 0;
            var mutation = new CachedContentMutation(node =>
            {
                if (node is TextContent<double> tc && tc.Text == "RemoveMe")
                {
                    rewriteCount++;
                    return null;
                }
                return node;
            });

            var pg2 = mutation.Apply(pg);
            var nodes = pg2.GetContentNodes<double>();
            Assert.Empty(nodes);
            
            // Should only have called mutation for the first occurrence of the form, 
            // second one should be cached as "null/removed".
            // Actually, `RecursivelyApply` calls `Apply(content)` which then calls `_mutation`.
            // Wait, how does it count?
            // If the form is same and transform is same, it hits cache.
            Assert.Equal(1, rewriteCount);
        }

        [Fact]
        public void CachedContentMutation_Throws_For_NonInvertibleFormTransform()
        {
            var f1 = new FormWriter(PageSize.LETTER);
            f1.Rect(0, 0, 10, 10).Fill();
            var formStream = f1.Complete();

            var pg = new PdfPage(PageSize.LETTER);
            using (var writer = pg.GetWriter())
            {
                // Non-invertible matrix (scale 0,0)
                writer.Save().Scale(0, 0).Form(formStream).Restore();
            }

            var mutation = new CachedContentMutation(node => node);

            Assert.Throws<PdfLexerException>(() => mutation.Apply(pg));
        }

        [Fact]
        public void Cache_Reuses_Rewritten_Forms_For_StructurallyEqual_TextClipping()
        {
            var f1 = new FormWriter(PageSize.LETTER);
            f1.Rect(0, 0, 10, 10).Fill();
            var formStream = f1.Complete();

            var pg = new PdfPage(PageSize.LETTER);
            using (var writer = pg.GetWriter())
            {
                // First reference with text clipping
                writer.Save()
                    .BeginText()
                    .Font(Standard14Font.GetHelvetica(), 12)
                    .Op(new Tr_Op<double>(7))
                    .Text("Clip")
                    .EndText()
                    .Form(formStream)
                    .Restore();

                // Second reference with identical text clipping but created independently
                writer.Save()
                    .BeginText()
                    .Font(Standard14Font.GetHelvetica(), 12)
                    .Op(new Tr_Op<double>(7))
                    .Text("Clip")
                    .EndText()
                    .Form(formStream)
                    .Restore();
            }

            var mutation = new CachedContentMutation(node => node);
            var pg2 = mutation.Apply(pg);

            var forms = pg2.GetContentNodes<double>().OfType<FormContent<double>>().ToList();
            Assert.Equal(2, forms.Count);
            Assert.Same(forms[0].Stream, forms[1].Stream);
        }

        [Fact]
        public void Cache_DoesNotCrossReuse_Between_PathAndTextClipping()
        {
            var f1 = new FormWriter(PageSize.LETTER);
            f1.Rect(0, 0, 10, 10).Fill();
            var formStream = f1.Complete();

            var pg = new PdfPage(PageSize.LETTER);
            using (var writer = pg.GetWriter())
            {
                // Path clipping
                writer.Save()
                    .Rect(0, 0, 50, 50)
                    .Clip()
                    .EndPathNoOp()
                    .Form(formStream)
                    .Restore();

                // Text clipping with same (identity) transform
                writer.Save()
                    .BeginText()
                    .Font(Standard14Font.GetHelvetica(), 12)
                    .Op(new Tr_Op<double>(7))
                    .Text("Clip")
                    .EndText()
                    .Form(formStream)
                    .Restore();
            }

            var mutation = new CachedContentMutation(node => node);
            var pg2 = mutation.Apply(pg);

            var forms = pg2.GetContentNodes<double>().OfType<FormContent<double>>().ToList();
            Assert.Equal(2, forms.Count);
            Assert.NotSame(forms[0].Stream, forms[1].Stream);
        }

        [Fact]
        public void Cache_Works_For_Reused_InnerForms_In_NestedForms()
        {
            // Inner form
            var fInner = new FormWriter(50, 50);
            fInner.Rect(0, 0, 10, 10).Fill();
            var innerStream = fInner.Complete();

            // Outer form 1
            var fOuter1 = new FormWriter(PageSize.LETTER);
            fOuter1.Form(innerStream);
            var outerStream1 = fOuter1.Complete();

            // Outer form 2 (different stream object but identical content)
            var fOuter2 = new FormWriter(PageSize.LETTER);
            fOuter2.Form(innerStream);
            var outerStream2 = fOuter2.Complete();

            int innerPathMutationCount = 0;
            var mutation = new CachedContentMutation(node =>
            {
                if (node is PathSequence<double> ps && ps.Paths.Count > 0 && ps.Paths[0].XPos == 0)
                {
                    innerPathMutationCount++;
                }
                return node;
            });

            // Apply to first outer form
            mutation.Apply(new List<IContentNode<double>> { 
                new FormContent<double> { Stream = outerStream1, GraphicsState = new GfxState<double>() } 
            });
            Assert.Equal(1, innerPathMutationCount);

            // Apply to second outer form
            mutation.Apply(new List<IContentNode<double>> { 
                new FormContent<double> { Stream = outerStream2, GraphicsState = new GfxState<double>() } 
            });
            
            // Should still be 1 if reuse worked
            Assert.Equal(1, innerPathMutationCount);
        }

        [Fact]
        public void CacheKey_Treats_StructurallyEqual_MultiSectionClipping_AsEquivalent()
        {
            var f1 = new FormWriter(PageSize.LETTER);
            f1.Rect(0, 0, 10, 10).Fill();
            var formStream = f1.Complete();

            var pg = new PdfPage(PageSize.LETTER);
            using (var writer = pg.GetWriter())
            {
                // First reference
                writer.Save()
                    .Rect(0, 0, 50, 50)
                    .Clip()
                    .EndPathNoOp()
                    .Form(formStream)
                    .Restore();

                // Second reference with SAME clipping path but created via separate operators
                writer.Save()
                    .Rect(0, 0, 50, 50)
                    .Clip()
                    .EndPathNoOp()
                    .Form(formStream)
                    .Restore();
            }

            var mutation = new CachedContentMutation(node => node);
            var pg2 = mutation.Apply(pg);

            var forms = pg2.GetContentNodes<double>().OfType<FormContent<double>>().ToList();
            Assert.Equal(2, forms.Count);
            Assert.Same(forms[0].Stream, forms[1].Stream);
        }

        [Fact]
        public void Mutation_Should_Descend_Into_MarkedContentGroups()
        {
            var tag = new MarkedContent(new PdfName("P"));
            var mcg = new MarkedContentGroup<double>(tag)
            {
                GraphicsState = new GfxState<double>()
            };
            mcg.Children.Add(new PathSequence<double> { 
                Paths = new List<SubPath<double>> { 
                    new SubPath<double> { 
                        XPos = 0, YPos = 0,
                        Operations = new List<IPathCreatingOp<double>>()
                    } 
                },
                GraphicsState = new GfxState<double>(),
                Closing = n_Op.Value
            });

            var mutation = new CachedContentMutation(node => {
                if (node is PathSequence<double> path) {
                    path.CompatibilitySection = true;
                    return node;
                }
                return node;
            });

            var mutated = mutation.Apply(new List<IContentNode<double>> { mcg });

            Assert.Single(mutated);
            Assert.IsType<MarkedContentGroup<double>>(mutated[0]);
            var newMcg = (MarkedContentGroup<double>)mutated[0];
            Assert.Single(newMcg.Children);
            var newPath = (PathSequence<double>)newMcg.Children[0];
            Assert.True(newPath.CompatibilitySection);
        }

        [Fact]
        public void Recursive_Mutation_Should_Preserve_Clipping()
        {
            // 1. Create a form with some content
            var f1 = new FormWriter(PageSize.LETTER);
            f1.Rect(0, 0, 10, 10).Fill();
            var formStream = f1.Complete();

            // 2. Create a page with the form and some clipping
            var pg = new PdfPage(PageSize.LETTER);
            using (var writer = pg.GetWriter())
            {
                writer.Save()
                    .Rect(new PdfRect<double> { LLx = 0, LLy = 0, URx = 150, URy = 150 })
                    .Clip(false)
                    .EndPathNoOp()
                    .Form(formStream)
                    .Restore();
            }

            // 3. Apply a mutation
            var mutation = new CachedContentMutation(node => {
                return node; // just return same
            });

            var pg2 = mutation.Apply(pg);

            // 4. Verify that the rewritten form's content has the clipping baked in
            var nodes = pg2.GetContentNodes<double>();
            var form = nodes.OfType<FormContent<double>>().Single();
            
            // Parse the rewritten form
            var formNodes = form.Parse();
            var pathNode = formNodes.OfType<PathSequence<double>>().Single();
            
            // It should have clipping from the parent form's reference
            Assert.NotNull(pathNode.GraphicsState.Clipping);
            Assert.NotEmpty(pathNode.GraphicsState.Clipping);
        }

        [Fact]
        public void End_To_End_Parse_Edit_Write_Should_Preserve_Structure()
        {
            // 1. Create a page with a form and marked content
            var pg = new PdfPage(PageSize.LETTER);
            using (var writer = pg.GetWriter())
            {
                ((ContentWriter<double>)writer).MarkedContent(new MarkedContent(new PdfName("P")));
                writer.Rect(0, 0, 10, 10).Fill();
                ((ContentWriter<double>)writer).EndMarkedContent();
            }

            // 2. Mutate
            var mutation = new CachedContentMutation(node => {
                if (node is PathSequence<double> path) {
                    path.CompatibilitySection = true;
                    return node;
                }
                return node;
            });

            var mutatedPage = mutation.Apply(pg);

            // 3. Save to bytes
            var doc = PdfDocument.Create();
            doc.Pages.Add(mutatedPage);
            var bytes = doc.Save();

            // 4. Parse back and verify
            using (var doc2 = PdfDocument.Open(bytes))
            {
                var pgBack = doc2.Pages[0];
                var nodes = pgBack.GetContentNodes<double>();
                
                Assert.Single(nodes);
                Assert.IsType<MarkedContentGroup<double>>(nodes[0]);
                var mcg = (MarkedContentGroup<double>)nodes[0];
                Assert.Equal("P", mcg.Tag.Name.Value);
                Assert.Single(mcg.Children);
                var path = (PathSequence<double>)mcg.Children[0];
                Assert.True(path.CompatibilitySection);
            }
        }
        [Fact]
        public void Cache_DoesNotReuse_When_ParentClippingStacksDiffer()
        {
            var f1 = new FormWriter(PageSize.LETTER);
            f1.Rect(0, 0, 10, 10).Fill();
            var formStream = f1.Complete();

            var pg = new PdfPage(PageSize.LETTER);
            using (var writer = pg.GetWriter())
            {
                // Clip 1
                writer.Save()
                    .Rect(0, 0, 50, 50)
                    .Clip()
                    .EndPathNoOp()
                    .Form(formStream)
                    .Restore();

                // Clip 2 - different rect
                writer.Save()
                    .Rect(0, 0, 60, 60)
                    .Clip()
                    .EndPathNoOp()
                    .Form(formStream)
                    .Restore();
            }

            var mutation = new CachedContentMutation(node => node);
            var pg2 = mutation.Apply(pg);

            var forms = pg2.GetContentNodes<double>().OfType<FormContent<double>>().ToList();
            Assert.Equal(2, forms.Count);
            // Should be different streams because parent clipping differs
            Assert.NotSame(forms[0].Stream, forms[1].Stream);
        }

    }
}
