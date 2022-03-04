#include "pch.h"
#include "ImageProps.h"
#if __has_include("ImageProps.g.cpp")
#include "ImageProps.g.cpp"
#endif

#include <filesystem>

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace winrt::Lucompressor_RTComp::implementation
{
    winrt::hstring ImageProps::ImgPath()
    {
        return m_imgpath;
    }

    void ImageProps::ImgPath(const winrt::hstring value)
    {
        m_imgpath = value;
    }

    int32_t ImageProps::ImgHeight()
    {
        return m_imgheight;
    }

    void ImageProps::ImgHeight(const int32_t value)
    {
        m_imgheight = value;
    }

    int32_t ImageProps::ImgWidth()
    {
        return m_imgwidth;
    }

    void ImageProps::ImgWidth(const int32_t value)
    {
        m_imgwidth = value;
    }

    double_t ImageProps::ImgQualityValue()
    {
        return m_imgqualityvalue;
    }

    void ImageProps::ImgQualityValue(const double_t value)
    {
        m_imgqualityvalue = value;
    }

    winrt::Windows::Foundation::IAsyncOperation<bool> ImageProps::CompressImageAndSave(const winrt::hstring DestinyPath, const int32_t Index)
    {
        co_await winrt::resume_background();

        if (DestinyPath.size() == 0 || ImgPath().size() == 0 ||
            !std::filesystem::exists(std::filesystem::path{ winrt::to_string(DestinyPath) }) ||
            !std::filesystem::exists(std::filesystem::path{ winrt::to_string(ImgPath()) }))
        {
            co_return false;
        }
        try
        {
            const winrt::Windows::Storage::StorageFile File_Ref = co_await winrt::Windows::Storage::StorageFile::GetFileFromPathAsync(ImgPath());
            const winrt::Windows::Storage::StorageFolder DestFolder_Ref = co_await winrt::Windows::Storage::StorageFolder::GetFolderFromPathAsync(DestinyPath);

            const winrt::Windows::Storage::StorageFile CopiedFile_Ref = co_await File_Ref.CopyAsync(DestFolder_Ref, L"Lucompressor_Img_" + winrt::to_hstring(Index) + L".jpg",
                winrt::Windows::Storage::NameCollisionOption::ReplaceExisting);

            winrt::Windows::Storage::Streams::IRandomAccessStream FileStream = co_await CopiedFile_Ref.OpenAsync(winrt::Windows::Storage::FileAccessMode::ReadWrite);

            const winrt::Windows::Graphics::Imaging::BitmapDecoder Decoder =
                co_await winrt::Windows::Graphics::Imaging::BitmapDecoder::CreateAsync(FileStream);

            const winrt::Windows::Graphics::Imaging::PixelDataProvider PixelData = co_await Decoder.GetPixelDataAsync();

            winrt::Windows::Graphics::Imaging::BitmapPropertySet EncodingProps;
            const winrt::Windows::Graphics::Imaging::BitmapTypedValue ImageQuality(winrt::box_value(ImgQualityValue() / 100), winrt::Windows::Foundation::PropertyType::Single);
            EncodingProps.Insert(L"ImageQuality", ImageQuality);

            FileStream.Size(0);

            winrt::Windows::Graphics::Imaging::BitmapEncoder Encoder =
                co_await winrt::Windows::Graphics::Imaging::BitmapEncoder::CreateAsync(winrt::Windows::Graphics::Imaging::BitmapEncoder::JpegEncoderId(), FileStream, EncodingProps);

            Encoder.SetPixelData(Decoder.BitmapPixelFormat(),
                Decoder.BitmapAlphaMode(),
                Decoder.OrientedPixelWidth(),
                Decoder.OrientedPixelHeight(),
                Decoder.DpiX(),
                Decoder.DpiY(),
                PixelData.DetachPixelData());

            Encoder.BitmapTransform().ScaledHeight(ImgHeight());
            Encoder.BitmapTransform().ScaledWidth(ImgWidth());

            co_await Encoder.FlushAsync();
            co_return FileStream.FlushAsync().get();
        }
        catch (...)
        {
            co_return false;
        }
    }

    winrt::Windows::Foundation::IAsyncAction ImageProps::LoadImageSizing()
    {
        co_await winrt::resume_background();

        if (ImgPath().size() == 0 ||
            !std::filesystem::exists(std::filesystem::path{ winrt::to_string(ImgPath()) }))
        {
            co_return;
        }

        try
        {
            const winrt::Windows::Storage::StorageFile File_Ref = co_await winrt::Windows::Storage::StorageFile::GetFileFromPathAsync(ImgPath());
            const winrt::Windows::Storage::Streams::IRandomAccessStream FileStream = co_await File_Ref.OpenAsync(winrt::Windows::Storage::FileAccessMode::Read);
            
            const winrt::Windows::Graphics::Imaging::BitmapDecoder Decoder =
                co_await winrt::Windows::Graphics::Imaging::BitmapDecoder::CreateAsync(FileStream);

            ImgHeight(Decoder.OrientedPixelHeight());
            ImgWidth(Decoder.OrientedPixelWidth());

            FileStream.Close();

            co_return;
        }
        catch (...)
        {
            co_return;
        }
    }
}
