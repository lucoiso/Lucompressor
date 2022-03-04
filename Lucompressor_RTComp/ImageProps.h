﻿#pragma once

#include "ImageProps.g.h"

namespace winrt::Lucompressor_RTComp::implementation
{
    struct ImageProps : ImagePropsT<ImageProps>
    {
        ImageProps() = default;

        winrt::hstring ImgPath();
        void ImgPath(const winrt::hstring value);

        int32_t ImgHeight();
        void ImgHeight(const int32_t value);

        int32_t ImgWidth();
        void ImgWidth(const int32_t value);

        double_t ImgQualityValue();
        void ImgQualityValue(const double_t value);

        winrt::Windows::Foundation::IAsyncOperation<bool> CompressImageAndSave(const winrt::hstring DestinyPath, const int32_t Index);
        winrt::Windows::Foundation::IAsyncAction LoadImageSizing();

    private:
        winrt::hstring m_imgpath = L"ms-appx:///Assets/Img_Placeholder.jpg";
        int32_t m_imgheight = 0;
        int32_t m_imgwidth = 0;
        double_t m_imgqualityvalue = 100;
    };
}

namespace winrt::Lucompressor_RTComp::factory_implementation
{
    struct ImageProps : ImagePropsT<ImageProps, implementation::ImageProps>
    {
    };
}
