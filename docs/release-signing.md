# 发布流水线：GitHub Actions + SignPath 免费代码签名

目标：推一个 tag，CI 自动编译、导出、**用 SignPath Foundation 的证书签名**，产出可直接下载的 zip。
拿到签名后，用户第一次运行不再出现"发布者未知"。

工作流文件：[.github/workflows/windows-release.yml](../.github/workflows/windows-release.yml)

---

## 一、三件硬性前置条件

### 1. 仓库必须公开，且有 OSI 认可的开源许可

SignPath Foundation 只签**开源项目**的构建产物，签署主体显示为 `SignPath Foundation`，它替你担保"这个二进制确实由该仓库构建"。

✅ 已就位：仓库根目录有 [LICENSE](../LICENSE)（**MIT**）。
⏳ 仍缺：git remote（仓库还没推到 GitHub）。

如需更换许可，可参考（改 LICENSE 与 README 的许可证小节即可）：

| 许可 | 含义 | 适合 |
|---|---|---|
| MIT（当前） | 任何人都能拿去商用、闭源分发，只需保留版权声明 | 想尽快过审、不介意别人复用 |
| GPL-3.0 | 衍生作品必须同样开源 | 想保持开源生态 |
| AGPL-3.0 | 连"网络服务"形态也必须开源 | 对"被人拿去改个皮上线"最敏感 |

**没有 LICENSE，SignPath 申请会被直接驳回。**

### 2. 公开托管仓库

```powershell
git remote add origin https://github.com/<你的账号>/Click_DeepSeek.git
git push -u origin master
```

### 3. SignPath 账号与项目

在 <https://signpath.io> 注册（Foundation 免费额度需要申请，见 [signpath.org/apply](https://signpath.org/apply)）。
审核通过后，控制台里要建三样东西，名字会填进 GitHub 变量。

---

## 二、SignPath 控制台怎么配

1. **Organization**：注册时创建，记下 **Organization ID**（设置页可见）。
2. **Project**：新建项目，slug 例如 `click-deepseek`。
3. **Artifact Configuration**：新建，规则大致如下
   - 匹配 `*.exe`：`Signing Enabled = true`，`Hash Algorithm = SHA-256`
   - 匹配 `*.dll`：按需开启（`GodotSharp.dll` 不建议签，它不是你的产物）
   - 其余文件：只做哈希
4. **Signing Policy**：新建，slug 例如 `release-signing`，勾选"允许 GitHub Actions 触发"。
5. **Build System Integration**：选 **GitHub Actions**，绑定到你的仓库。
   SignPath 靠这个做**来源校验**：它会核对 artifact 是不是从该仓库的 workflow 产出的。
6. **API Token**：Personal Access Token 页面生成，只勾选需要的权限。

---

## 三、GitHub 仓库配置

Settings → Secrets and variables → Actions：

| 类型 | 名称 | 值 |
|---|---|---|
| Secret | `SIGNPATH_API_TOKEN` | SignPath 生成的 token |
| Secret | `SIGNPATH_ORGANIZATION_ID` | SignPath Organization ID |
| Variable | `SIGNPATH_PROJECT_SLUG` | 例如 `click-deepseek` |
| Variable | `SIGNPATH_SIGNING_POLICY_SLUG` | 例如 `release-signing` |

> `SIGNPATH_PROJECT_SLUG` 同时是开关：**没配它，签名步骤自动跳过**，流水线仍然会跑完并产出未签名 zip。
> 建议先用这种方式跑通构建，再补签名。

---

## 四、触发

```powershell
git tag v0.3.1
git push origin v0.3.1
```

- `v*` tag → 构建 + 签名 + 自动发 GitHub Release；
- 手动 `workflow_dispatch` → 只构建 + 签名，产物在 Actions 的 artifacts 里。

流水线各步：装 .NET 9 → 装 Godot 4.7.2 .NET 版与 mono 导出模板 → `dotnet build`（带 PathMap，剔除 runner 绝对路径）→ `godot --headless --export-release` → 上传未签名 artifact → SignPath 签名 → 打包 zip → 发 Release。

---

## 五、验证是否真的签上了

```powershell
Get-AuthenticodeSignature .\ClickDeepSeek.exe | Format-List Status, StatusMessage, @{n='Signer';e={$_.SignerCertificate.Subject}}
signtool verify /pa /v .\ClickDeepSeek.exe
```

期望：`Status = Valid`，`Signer` 是 `SignPath Foundation`（若用自有证书则是你的主体名）。
`StatusMessage` 里带 "timestamp" 字样说明时间戳生效，证书到期后旧版本仍然有效。

---

## 六、失败排查

| 现象 | 原因 | 处理 |
|---|---|---|
| `No export template found` | 模板版本目录名不对 | 目录必须是 `%APPDATA%\Godot\export_templates\4.7.2.stable.mono\`，里面直接是 `windows_release_x86_64.exe` |
| 导出报 `no solution file was found` | 仓库缺 `ClickDeepSeek.sln` | 本仓库已补上；如被删掉会复现 |
| SignPath 报 origin verification failed | artifact 不是该仓库 workflow 产出的，或 Build System Integration 没绑仓库 | 检查 SignPath 里的 GitHub 绑定 |
| `No matching artifact` | artifact 名与配置规则不符 | 确认上传的是 `build/stage/**`，且 exe 在根层 |
| 签名步骤被跳过 | 没配 `SIGNPATH_PROJECT_SLUG` 变量 | 补变量后重跑 |
| 用户仍看到"未知发布者" | 下载后被 MOTW 标记 / 或 Reputation 未建立 | 提示"解除锁定"；签名信誉需要几次下载累积 |

---

## 七、如果你决定不开源

SignPath Foundation 这条路直接作废（它只签开源项目）。替代方案是自购证书，改动只有一处：
把 workflow 里的 `Sign with SignPath` 步骤换成

```yaml
- name: Sign with signtool
  shell: pwsh
  run: |
    signtool sign /fd SHA256 /td SHA256 /tr http://timestamp.digicert.com `
      /f "${{ secrets.CODESIGN_PFX }}" /p "${{ secrets.CODESIGN_PFX_PASSWORD }}" `
      "build/stage/$env:PROJECT_NAME.exe"
```

并把 pfx 以 base64 存进 secret。注意：**OV 证书新签的头几周，SmartScreen 依然会拦**，
要靠下载量积累信誉；只有 EV 证书是即签即生效。
