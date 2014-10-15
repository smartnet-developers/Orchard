﻿using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.Layouts.Framework.Display;
using Orchard.Layouts.Framework.Drivers;
using Orchard.Layouts.ViewModels;
using ContentItem = Orchard.Layouts.Elements.ContentItem;

namespace Orchard.Layouts.Drivers {
    public class ContentItemDriver : ElementDriver<ContentItem> {
        private readonly IContentManager _contentManager;

        public ContentItemDriver(IContentManager contentManager) {
            _contentManager = contentManager;
        }

        protected override EditorResult OnBuildEditor(ContentItem element, ElementEditorContext context) {
            var layoutPart = context.Content;
            var viewModel = new ContentItemEditorViewModel();
            var editor = context.ShapeFactory.EditorTemplate(TemplateName: "Elements.ContentItem", Model: viewModel);

            if (context.Updater != null) {
                context.Updater.TryUpdateModel(viewModel, context.Prefix, null, null);
                element.ContentItemIds = ContentItem.Deserialize(viewModel.ContentItemIds);
                element.DisplayType = viewModel.DisplayType;
            }

            var contentItemIds = element.ContentItemIds;
            var displayType = element.DisplayType;

            viewModel.ContentItems = GetContentItems(RemoveCurrentContentItemId(contentItemIds, layoutPart.Id)).ToArray();
            viewModel.DisplayType = displayType;

            return Editor(context, editor);
        }

        protected override void OnDisplaying(ContentItem element, ElementDisplayContext context) {
            var contentItemIds = RemoveCurrentContentItemId(element.ContentItemIds, context.Content.Id);
            var displayType = element.DisplayType;
            var contentItems = GetContentItems(contentItemIds).ToArray();
            var contentItemShapes = contentItems.Select(x => _contentManager.BuildDisplay(x, displayType)).ToArray();

            context.ElementShape.ContentItems = contentItemShapes;
        }

        protected IEnumerable<ContentManagement.ContentItem> GetContentItems(IEnumerable<int> ids) {
            return _contentManager.GetMany<IContent>(ids, VersionOptions.Published, QueryHints.Empty).Select(x => x.ContentItem);
        }

        // The user can't pick the content that will host the selected content to prevent an infinite loop / stack overflow.
        protected IEnumerable<int> RemoveCurrentContentItemId(IEnumerable<int> ids, int currentId) {
            return ids.Where(x => x != currentId);
        }
    }
}