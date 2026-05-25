@echo off
chcp 65001 >nul
setlocal enabledelayedexpansion

echo ========================================
echo   BehaviorTree 迁移工具
echo ========================================
echo.

:: 获取当前脚本所在目录（即 Package 的根目录）
set "SCRIPT_DIR=%~dp0"
set "SCRIPT_DIR=%SCRIPT_DIR:~0,-1%"

:: 查找项目根目录（向上查找包含 Assets 文件夹的目录）
set "PROJECT_ROOT=%SCRIPT_DIR%"
:find_root
if exist "%PROJECT_ROOT%\Assets" goto found_root
set "PROJECT_ROOT=%PROJECT_ROOT%\.."
if "%PROJECT_ROOT%"=="%PROJECT_ROOT%\.." goto not_found
goto find_root

:not_found
echo [错误] 未找到 Assets 文件夹，请确保此脚本位于 Unity Package 内
pause
exit /b 1

:found_root
echo [信息] 项目根目录: %PROJECT_ROOT%

:: 定义源目录和目标目录
set "SOURCE_DIR=%SCRIPT_DIR%\BehaviorTree"
set "DEST_DIR=%PROJECT_ROOT%\Assets\BehaviorTree"

:: 检查源目录是否存在
if not exist "%SOURCE_DIR%" (
    echo [错误] 源目录不存在: %SOURCE_DIR%
    echo [信息] 请确保此脚本位于 Package 根目录，且存在 BehaviorTree 子文件夹
    pause
    exit /b 1
)

:: 检查目标目录是否已存在（可选：添加覆盖确认）
if exist "%DEST_DIR%" (
    echo [警告] 目标目录已存在: %DEST_DIR%
    set /p "confirm=是否覆盖？(Y/N): "
    if /i not "!confirm!"=="Y" (
        echo 已取消迁移
        pause
        exit /b 0
    )
    rmdir /s /q "%DEST_DIR%"
    echo [信息] 已删除旧目录
)

echo.
echo [信息] 源目录: %SOURCE_DIR%
echo [信息] 目标目录: %DEST_DIR%
echo.
echo 开始复制文件...

:: 使用 robocopy 复制文件（Windows 自带，功能强大）
:: /E 包含子目录，/NFL /NDL /NJH /NJS 减少输出噪音
robocopy "%SOURCE_DIR%" "%DEST_DIR%" /E /NFL /NDL /NJH /NJS

if %errorlevel% geq 8 (
    echo [错误] 复制过程中发生错误
    pause
    exit /b 1
)

echo [成功] 文件复制完成

:: 可选：删除源目录（Package 内的 BehaviorTree 文件夹）
echo.
set /p "delete_source=是否删除 Package 内的源文件？(Y/N): "
if /i "!delete_source!"=="Y" (
    rmdir /s /q "%SOURCE_DIR%"
    if exist "%SOURCE_DIR%" (
        echo [警告] 删除失败，可能有只读文件
    ) else (
        echo [信息] 已删除 Package 内的源文件
    )
)

echo.
echo ========================================
echo   迁移完成！
echo   文件已复制到: Assets\BehaviorTree
echo ========================================
pause