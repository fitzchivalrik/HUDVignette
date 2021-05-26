﻿#if DEBUG
using System;
using System.Runtime.InteropServices;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using FFXIVClientStructs.FFXIV.Component.GUI;
using FFXIVClientStructs.FFXIV.Component.GUI.ULD;

namespace SimpleTweaksPlugin.Helper {
    public static unsafe partial class UiHelper {
        
        public static AtkUnitBase* CloneNode(AtkUnitBase* original,
            bool keepObjects = false,
            bool keepTextures = false,
            bool keepParts = false)
        {
            var size = sizeof(AtkUnitBase);

            var allocation = Alloc((ulong)size);
            var unitBaseBytes = new byte[size];
            Marshal.Copy(new IntPtr(original), unitBaseBytes, 0, unitBaseBytes.Length);
            Marshal.Copy(unitBaseBytes, 0, allocation, unitBaseBytes.Length);

            var newNode = (AtkUnitBase*)allocation;
            //TODO No copying of component nodes yet
            newNode->WindowNode = null;
            newNode->CollisionNodeList = null;
            newNode->CollisionNodeListCount = 0;
            //TODO I imagine this needs to be somehow treated too.
            newNode->UldManager.ComponentData = null;
            // Handle to the ULD file?
            newNode->UldManager.UldResourceHandle = null;
            // TODO ?? ?? Crashes of null
            //newNode->UldManager.AtkResourceRendererManager = null;
            newNode->UldManager.ComponentData = null;
            // TODO Only set in ComponentNodes ULD Data?
            newNode->UldManager.RootNode  = null;
            newNode->UldManager.RootNodeWidth = 0;
            newNode->UldManager.RootNodeHeight = 0;
            if (!keepObjects)
            {
                newNode->UldManager.Objects = null;
                newNode->UldManager.ObjectCount = 0;
            }

            if (!keepTextures)
            {
                newNode->UldManager.Assets = null;
                newNode->UldManager.AssetCount = 0;
            }

            if (!keepParts)
            {
                // TODO Any sense in keeping parts without textures?
                newNode->UldManager.PartsList = null;
                newNode->UldManager.PartsListCount = 0;
            }

            newNode->UldManager.NodeListCount = 0;
            SimpleLog.Log($"Original has Rootnode? {original->RootNode != null}");
            var newSize = original->RootNode == null ? 0 : 1; 
            newNode->UldManager.NodeList = (AtkResNode**)Alloc((ulong)((newSize + 1) * 8));
            newNode->UldManager.NodeList[newNode->UldManager.NodeListCount++] = original->RootNode == null ? null : CloneNode(original->RootNode);
            newNode->RootNode = newNode->UldManager.NodeList[0];
            return newNode;
        }
        
        
        //TODO END ADDITIONS
        public static void Hide(AtkResNode* node) {
            node->Flags &= ~0x10;
            node->Flags_2 |= 0x1;
        }
        public static void Show(AtkResNode* node) {
            node->Flags |= 0x10;
            node->Flags_2 |= 0x1;
        }

        public static void SetVisible(AtkResNode* node, bool visible) {
            if (visible) {
                Show(node);
            } else {
                Hide(node);
            }
        }

        public static void SetText(AtkTextNode* textNode, SeString str) {
            if (!Ready) return;
            var bytes = str.Encode();
            var ptr = Marshal.AllocHGlobal(bytes.Length + 1);
            Marshal.Copy(bytes, 0, ptr, bytes.Length);
            Marshal.WriteByte(ptr, bytes.Length, 0);
            _atkTextNodeSetText(textNode, (byte*) ptr);
            Marshal.FreeHGlobal(ptr);
        }

        public static void SetText(AtkTextNode* textNode, string str) {
            if (!Ready) return;
            var seStr = new SeString(new Payload[] { new TextPayload(str) });
            SetText(textNode, seStr);
        }

        public static void SetSize(AtkResNode* node, int? width, int? height) {
            if (width != null && width >= ushort.MinValue && width <= ushort.MaxValue) node->Width = (ushort) width.Value;
            if (height != null && height >= ushort.MinValue && height <= ushort.MaxValue) node->Height = (ushort) height.Value;
            node->Flags_2 |= 0x1;
        }

        public static void SetPosition(AtkResNode* node, float? x, float? y) {
            if (x != null) node->X = x.Value;
            if (y != null) node->Y = y.Value;
            node->Flags_2 |= 0x1;
        }
        
        public static void SetPosition(AtkUnitBase* atkUnitBase, float? x, float? y) {
            if (x >= short.MinValue && x <= short.MaxValue) atkUnitBase->X = (short) x.Value;
            if (y >= short.MinValue && x <= short.MaxValue) atkUnitBase->Y = (short) y.Value;
        }

        public static void SetWindowSize(AtkComponentNode* windowNode, ushort? width, ushort? height) {
            if (((ULDComponentInfo*) windowNode->Component->UldManager.Objects)->ComponentType != ComponentType.Window) return;

            width ??= windowNode->AtkResNode.Width;
            height ??= windowNode->AtkResNode.Height;

            if (width < 64) width = 64;

            SetSize(windowNode, width, height);  // Window
            var n = windowNode->Component->UldManager.RootNode;
            SetSize(n, width, height);  // Collision
            n = n->PrevSiblingNode;
            SetSize(n, (ushort)(width - 14), null); // Header Collision
            n = n->PrevSiblingNode;
            SetSize(n, width, height); // Background
            n = n->PrevSiblingNode;
            SetSize(n, width, height); // Focused Border
            n = n->PrevSiblingNode;
            SetSize(n, (ushort) (width - 5), null); // Header Node
            n = n->ChildNode;
            SetSize(n, (ushort) (width - 20), null); // Header Seperator
            n = n->PrevSiblingNode;
            SetPosition(n, width - 33, 6); // Close Button
            n = n->PrevSiblingNode;
            SetPosition(n, width - 47, 8); // Gear Button
            n = n->PrevSiblingNode;
            SetPosition(n, width - 61, 8); // Help Button

            windowNode->AtkResNode.Flags_2 |= 0x1;
        }

        public static void ExpandNodeList(AtkComponentNode* componentNode, ushort addSize) {
            var newNodeList = ExpandNodeList(componentNode->Component->UldManager.NodeList, componentNode->Component->UldManager.NodeListCount, (ushort) (componentNode->Component->UldManager.NodeListCount + addSize));
            componentNode->Component->UldManager.NodeList = newNodeList;
        }

        public static void ExpandNodeList(AtkUnitBase* atkUnitBase, ushort addSize) {
            var newNodeList = ExpandNodeList(atkUnitBase->UldManager.NodeList, atkUnitBase->UldManager.NodeListCount, (ushort)(atkUnitBase->UldManager.NodeListCount + addSize));
            atkUnitBase->UldManager.NodeList = newNodeList;
        }

        private static AtkResNode** ExpandNodeList(AtkResNode** originalList, ushort originalSize, ushort newSize = 0) {
            if (newSize <= originalSize) newSize = (ushort)(originalSize + 1);
            var oldListPtr = new IntPtr(originalList);
            var newListPtr = Alloc((ulong)((newSize + 1) * 8));
            var clone = new IntPtr[originalSize];
            Marshal.Copy(oldListPtr, clone, 0, originalSize);
            Marshal.Copy(clone, 0, newListPtr, originalSize);
            return (AtkResNode**)(newListPtr);
        }

        public static AtkResNode* CloneNode(AtkResNode* original) {
            var size = original->Type switch
            {
                NodeType.Res => sizeof(AtkResNode),
                NodeType.Image => sizeof(AtkImageNode),
                NodeType.Text => sizeof(AtkTextNode),
                NodeType.NineGrid => sizeof(AtkNineGridNode),
                NodeType.Counter => sizeof(AtkCounterNode),
                NodeType.Collision => sizeof(AtkCollisionNode),
                _ => throw new Exception($"Unsupported Type: {original->Type}")
            };

            var allocation = Alloc((ulong)size);
            var bytes = new byte[size];
            Marshal.Copy(new IntPtr(original), bytes, 0, bytes.Length);
            Marshal.Copy(bytes, 0, allocation, bytes.Length);

            var newNode = (AtkResNode*)allocation;
            newNode->ParentNode = null;
            newNode->ChildNode = null;
            newNode->ChildCount = 0;
            newNode->PrevSiblingNode = null;
            newNode->NextSiblingNode = null;
            return newNode;
        }

        public static void Close(AtkUnitBase* atkUnitBase, bool unknownBool = false) {
            if (!Ready) return;
            _atkUnitBaseClose(atkUnitBase, (byte) (unknownBool ? 1 : 0));
        }

        public static T* CleanAlloc<T>() where T : unmanaged {
            var alloc = Alloc(sizeof(T));
            Marshal.Copy(new byte[sizeof(T)], 0, alloc, sizeof(T));
            return (T*) alloc;
        }
    }
}
#endif