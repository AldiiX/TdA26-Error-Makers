<script setup lang="ts">

</script>

<template>
    <div :class="$style.centerdiv">
        <h1>&lt;Hello TdA/&gt;</h1>
        <p>we are error makers</p>
    </div>
</template>

<style module lang="scss">
@use '../app' as app;



.centerdiv {
    display: flex;
    justify-content: center;
    align-items: center;
    height: 100vh; // Full viewport height
    text-align: center;
    flex-direction: column;

    * {
        margin: 0;
    }

    h1 {
        font-size: 128px;
        margin-bottom: 32px;
    }

    p {
        font-size: 24px;
    }
}



// Laptops Responsive
@media screen and (max-width: app.$laptopBreakpoint) {

}

// tablet
@media screen and (max-width: app.$tabletBreakpoint) {

}

// mobile
@media screen and (max-width: app.$mobileBreakpoint) {

}
</style>