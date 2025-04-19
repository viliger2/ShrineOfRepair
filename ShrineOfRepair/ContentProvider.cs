using R2API;
using RoR2;
using RoR2.ContentManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ShrineOfRepair
{
    public class ContentProvider : IContentPackProvider
    {
        public string identifier => ShrineOfRepairPlugin.GUID + "." + nameof(ContentProvider);

        private readonly ContentPack _contentPack = new ContentPack();

        public const string AssetBundleName = "shrinerepair";
        public const string AssetBundleFolder = "AssetBundles";

        public static Dictionary<string, Material> MaterialCache = new Dictionary<string, Material>();

        public static readonly Dictionary<string, string> ShaderLookup = new Dictionary<string, string>()
        {
            {"stubbedror2/base/shaders/hgstandard", "RoR2/Base/Shaders/HGStandard.shader"},
            {"stubbedror2/base/shaders/hgsnowtopped", "RoR2/Base/Shaders/HGSnowTopped.shader"},
            {"stubbedror2/base/shaders/hgtriplanarterrainblend", "RoR2/Base/Shaders/HGTriplanarTerrainBlend.shader"},
            {"stubbedror2/base/shaders/hgintersectioncloudremap", "RoR2/Base/Shaders/HGIntersectionCloudRemap.shader" },
            {"stubbedror2/base/shaders/hgcloudremap", "RoR2/Base/Shaders/HGCloudRemap.shader" },
            {"stubbedror2/base/shaders/hgopaquecloudremap", "RoR2/Base/Shaders/HGOpaqueCloudRemap.shader" },
            {"stubbedror2/base/shaders/hgdistortion", "RoR2/Base/Shaders/HGDistortion.shader" },
            {"stubbedcalm water/calmwater - dx11 - doublesided", "Calm Water/CalmWater - DX11 - DoubleSided.shader" },
            {"stubbedcalm water/calmwater - dx11", "Calm Water/CalmWater - DX11.shader" },
            {"stubbednature/speedtree", "RoR2/Base/Shaders/SpeedTreeCustom.shader"},
            {"stubbeddecalicious/decaliciousdeferreddecal", "Decalicious/DecaliciousDeferredDecal.shader" },
            {"stubbedror2/base/shaders/hgdamagenumber", "RoR2/Base/Shaders/HGDamageNumber.shader" },
            {"stubbedror2/base/shaders/hguianimatealpha", "RoR2/Base/Shaders/HGUIAnimateAlpha.shader" }
        };

        public IEnumerator FinalizeAsync(FinalizeAsyncArgs args)
        {
            args.ReportProgress(1f);
            yield break;
        }

        public IEnumerator GenerateContentPackAsync(GetContentPackAsyncArgs args)
        {
            ContentPack.Copy(_contentPack, args.output);
            args.ReportProgress(1f);
            yield break;
        }

        public IEnumerator LoadStaticContentAsync(LoadStaticContentAsyncArgs args)
        {
            _contentPack.identifier = identifier;

            string assetBundleFolderPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(typeof(ContentProvider).Assembly.Location), AssetBundleFolder);

            AssetBundle assetbundle = null;
            yield return LoadAssetBundle(System.IO.Path.Combine(assetBundleFolderPath, AssetBundleName), args.progressReceiver, (resultAssetBundle) => assetbundle = resultAssetBundle);

            yield return LoadAllAssetsAsync(assetbundle, args.progressReceiver, (Action<Sprite[]>)((assets) =>
            {
                if (ModCompat.RiskOfOptionsCompat.enabled)
                {
                    var icon = assets.First(asset => asset.name == "texRiskOfOptionsIcon");
                    ModCompat.RiskOfOptionsCompat.SetIcon(icon);
                }
            }));


            yield return LoadAllAssetsAsync(assetbundle, args.progressReceiver, (Action<Material[]>)((assets) =>
            {
                var materials = assets;

                if (materials != null)
                {
                    foreach (Material material in materials)
                    {
                        var replacementShader = Addressables.LoadAssetAsync<Shader>(ShaderLookup[material.shader.name.ToLower()]).WaitForCompletion();
                        if (replacementShader)
                        {
                            material.shader = replacementShader;
                        }
                        else
                        {
                            Log.Info("Couldn't find replacement shader for " + material.shader.name.ToLower());
                        }
                        MaterialCache.Add(material.name, material);
                    }
                }
            }));

            yield return LoadAllAssetsAsync(assetbundle, args.progressReceiver, (Action<GameObject[]>)((assets) =>
            {
                var shrinePrefabGood = assets.First(assets => assets.name == "ShrineRepairGood");
                var shrinePrefabSnow = assets.First(assets => assets.name == "ShrineRepairGood_snowy");
                var shrinePrefabSand = assets.First(assets => assets.name == "ShrineRepairGood_sandy");
                if (Modules.ShrineOfRepairConfigManager.UseBadModel.Value)
                {
                    shrinePrefabGood = assets.First(assets => assets.name == "ShrineRepair");
                    shrinePrefabSnow = assets.First(assets => assets.name == "ShrineRepair");
                    shrinePrefabSand = assets.First(assets => assets.name == "ShrineRepair");
                }

                var shrineStuff = new ShrineOfRepairStuff();

                if (Modules.ShrineOfRepairConfigManager.UsePickupPickerPanel.Value)
                {
                    shrinePrefabGood = shrineStuff.CreateShrineOfRepairPickerPrefab(shrinePrefabGood);
                    shrinePrefabSnow = shrineStuff.CreateShrineOfRepairPickerPrefab(shrinePrefabSnow);
                    shrinePrefabSand = shrineStuff.CreateShrineOfRepairPickerPrefab(shrinePrefabSand);
                }
                else 
                {
                    shrinePrefabGood = shrineStuff.CreateShrineOfRepairPurchasePrefab(shrinePrefabGood);
                    shrinePrefabSnow = shrineStuff.CreateShrineOfRepairPurchasePrefab(shrinePrefabSnow);
                    shrinePrefabSand = shrineStuff.CreateShrineOfRepairPurchasePrefab(shrinePrefabSand);
                }

                var cardHolderGood = shrineStuff.CreateCardHolder(shrinePrefabGood, "ShrineOfRepair");
                var cardHolderSnow = shrineStuff.CreateCardHolder(shrinePrefabSnow, "ShrineOfRepairSnowy");
                var cardHolderSand = shrineStuff.CreateCardHolder(shrinePrefabSand, "ShrineOfRepairSandy");

                ShrineOfRepairStuff.normalShrineSpawnCard = (InteractableSpawnCard)cardHolderGood.Card.spawnCard;

                foreach(var stage in shrineStuff.GetNormalStageList())
                {
                    DirectorAPI.Helpers.AddNewInteractableToStage(cardHolderGood, stage);
                }

                foreach (var stage in shrineStuff.GetSandyStageList())
                {
                    DirectorAPI.Helpers.AddNewInteractableToStage(cardHolderSand, stage);
                }

                foreach (var stage in shrineStuff.GetSnowyStageList())
                {
                    DirectorAPI.Helpers.AddNewInteractableToStage(cardHolderSnow, stage);
                }

                _contentPack.networkedObjectPrefabs.Add(new GameObject[] { shrinePrefabGood, shrinePrefabSand, shrinePrefabSnow });
            }));
        }

        private IEnumerator LoadAssetBundle(string assetBundleFullPath, IProgress<float> progress, Action<AssetBundle> onAssetBundleLoaded)
        {
            var assetBundleCreateRequest = AssetBundle.LoadFromFileAsync(assetBundleFullPath);
            while (!assetBundleCreateRequest.isDone)
            {
                progress.Report(assetBundleCreateRequest.progress);
                yield return null;
            }

            onAssetBundleLoaded(assetBundleCreateRequest.assetBundle);

            yield break;
        }

        private static IEnumerator LoadAllAssetsAsync<T>(AssetBundle assetBundle, IProgress<float> progress, Action<T[]> onAssetsLoaded) where T : UnityEngine.Object
        {
            var sceneDefsRequest = assetBundle.LoadAllAssetsAsync<T>();
            while (!sceneDefsRequest.isDone)
            {
                progress.Report(sceneDefsRequest.progress);
                yield return null;
            }

            onAssetsLoaded(sceneDefsRequest.allAssets.Cast<T>().ToArray());

            yield break;
        }
    }
}
